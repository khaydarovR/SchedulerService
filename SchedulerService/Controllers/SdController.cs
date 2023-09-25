using IronXL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVVMapp.App.Models;
using SchedulerService.Helpers;
using System.Data;
using System.Net;
using SchedulerService.Helpers;

namespace SchedulerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SdStudController : ControllerBase
    {

        private IEnumerable<Lesson> MainParsing(string group, int kurs, DateTime day, int? subgroup)
        {
            var lessons = new List<Lesson>();
            //Load Excel file
            WorkBook wb = ParserHelper.GetLatestUploadedWB();

            WorkSheet ws = wb.GetWorkSheet(kurs.ToString());

            var needCell = ws["B1:BH1"].FirstOrDefault(g => g.Text.Contains(group.ToString()));

            if (needCell == null)
            {
                throw new Exception($"Группа {group} не найдена");
            }

            var snake = new Locator(needCell);
            var delta = 2 + ((int)day.DayOfWeek - 1) * 15;
            var startPos = snake.MoveDown(delta).GetNewLocation();
            var endPos = snake
                .MoveUp(delta)
                .MoveDown(delta + 14)
                .GetNewLocation();

            var rows = ws[$"{startPos}:{endPos}"].Where(s => !string.IsNullOrEmpty(s.Text)).ToList();
            var weekTypeInNeedDate = ParserHelper.IsNeedWeekType(day);
            var needChar = weekTypeInNeedDate == WeekTypeEnum.Вверхняя ? "в" : "н";

            foreach ( var ce in rows )
            {

                snake = new Locator(ce);
                snake.MoveLeft(2);

                var curentLesson = new Lesson();
                var info = ws[snake.GetNewLocation() + ":" + snake.MoveRight(8).GetNewLocation()].Select(c => c.Text).ToList();

               
                if (needChar != info[1])
                {
                    continue;
                }

                curentLesson.Name = info[2];

                string timeString;
                try
                {
                    timeString = info[0].Split(' ')[1];
                }
                catch
                {
                    timeString = info[1];
                }
  
                DateTime res;
                if(DateTime.TryParse(timeString, out res))
                {
                    curentLesson.StartTime = res;
                }
                else
                {
                    timeString = info[0];
                    curentLesson.StartTime = DateTime.Parse(timeString);
                }
                curentLesson.LessonTypeEnum = info[5] == "лекция" ? LessonTypeEnum.Лекция : LessonTypeEnum.Лабы;
                curentLesson.TeacherName = info[8];

                if (info[1] == needChar)
                {
                    if (subgroup != null && curentLesson.Name.Contains($"гр.{subgroup}"))
                    {
                        int position = 0;
                        curentLesson.Name = ParserHelper.GetGroupDescription(curentLesson.Name, subgroup!.ToString(), out position);
                        curentLesson.TeacherName = ParserHelper.GetSplitItemFromPos(curentLesson.TeacherName, position);
                        var kab = ParserHelper.GetSplitItemFromPos(info[4], position);
                        curentLesson.Locate = info[3] + " " + kab;

                        lessons.Add(curentLesson);
                    }
                    else
                    {
                        if(curentLesson.LessonTypeEnum == LessonTypeEnum.Лекция)
                        {
                            curentLesson.Locate = info[3] + " " + info[4];
                            lessons.Add(curentLesson);
                        }
                    }
                }
            }

            return lessons;
        }


        [HttpPost("Gen")]
        public IActionResult Get(string group,[FromBody] DateTime date, int? subgroup)
        {
            int page = ParserHelper.FindPage(group);

            if (page == -1)
            {
                return BadRequest(group +" группа не найдена");
            }

            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                return Ok(new List<Lesson>());
            }

            var lessons = MainParsing(group, page, new DateTime(date.Year, date.Month, date.Day), subgroup);

            if (lessons == null)
            {
                return BadRequest("Пары не найдены");
            }
            return Ok(lessons);
        }
    
    }
}
