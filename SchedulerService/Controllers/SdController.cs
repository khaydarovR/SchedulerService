using IronXL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVVMapp.App.Models;
using SchedulerService.Services;
using System.Data;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace SchedulerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SdStudController : ControllerBase
    {
        private readonly WeekTypeEnum WeekTypeFirstSeptember = WeekTypeEnum.Нижняя;

        public SdStudController()
        {
            
        }

        [HttpGet("LoadToServ")]
        public IActionResult LoadToServ()
        {
            var url = @"https://kpfu.ru/portal/docs/F_154697384/Raspisanie.uchebnykh.zanyatij.EO.na.osennij.semestr.2023.24.uch.god_2_.xls";
            var type = @"application/vnd.ms-excel";

            using (WebClient client = new WebClient())
            {
                try
                {
                    byte[] fileData = client.DownloadData(url);
                    var stream = new MemoryStream(fileData);
                    var file = new FormFile(stream, 0, stream.Length, "sh", "sh.xls");

                    // Проверяем, что файл существует и не пустой
                    if (file != null && file.Length > 0)
                    {
                        // Определяем путь сохранения файла
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", $"sh{DateTime.Now.ToShortDateString()}.xls");

                        // Сохраняем файл на сервере
                        using (var stream2 = new FileStream(path, FileMode.Create))
                        {
                            file.CopyTo(stream2);
                        }

                        return Content("Файл успешно сохранен.");
                    }

                    return Content("Не удалось сохранить файл.");

                }
                catch (Exception ex)
                {
                    // Обрабатываем ошибку, если не удалось загрузить файл
                    return Content("Ошибка загрузки файла: " + ex.Message);
                }
            }
        }

        private IEnumerable<Lesson> GetLessons(string group, int kurs, DateTime day, int? subgroup)
        {
            var lessons = new List<Lesson>();
            //Load Excel file
            WorkBook wb = GetWB();

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
            var weekTypeInNeedDate = IsNeedWeekType(day);

            foreach ( var ce in rows )
            {
                snake = new Locator(ce);
                snake.MoveLeft(2);

                var curentLesson = new Lesson();
                var info = ws[snake.GetNewLocation() + ":" + snake.MoveRight(8).GetNewLocation()].Select(c => c.Text).ToList();

                curentLesson.Name = info[2];
                var timeString = info[0].Split(' ')[1];
                curentLesson.StartTime = DateTime.Parse(timeString);
                curentLesson.LessonTypeEnum = info[5] == "лекция" ? LessonTypeEnum.Лекция : LessonTypeEnum.Лабы;
                curentLesson.TeacherName = info[8];

                var needChar = weekTypeInNeedDate == WeekTypeEnum.Вверхняя ? "в" : "н";
                if (info[1] == needChar)
                {
                    if (subgroup != null && curentLesson.Name.Contains($"гр.{subgroup}"))
                    {
                        int position = 0;
                        curentLesson.Name = GetGroupDescription(curentLesson.Name, subgroup!.ToString(), out position);
                        curentLesson.TeacherName = GetSplitItemFromPos(curentLesson.TeacherName, position);
                        var kab = GetSplitItemFromPos(info[4], position);
                        curentLesson.Locate = info[3] + " " + kab;

                        lessons.Add(curentLesson);
                    }
                    else
                    {
                        curentLesson.Locate = info[3] + " " + info[4];
                        lessons.Add(curentLesson);
                    }
                }
            }

            return lessons;
        }

        private WeekTypeEnum IsNeedWeekType(DateTime date)
        {
            var res = WeekTypeCalculator.CalculateWeekType(date, WeekTypeFirstSeptember);
            return res;
        }

        private static WorkBook GetWB()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", $"sh18.09.2023.xls");
            WorkBook wb = WorkBook.Load(path);
            return wb;
        }

        [HttpPost("Gen")]
        public IActionResult Get(string group,[FromBody] DateTime date, int? subgroup)
        {
            int page = FindPage(group);

            if (page == -1)
            {
                return BadRequest(group +" группа не найдена");
            }

            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                return Ok(new List<Lesson>());
            }

            var lessons = GetLessons(group, page, new DateTime(date.Year, date.Month, date.Day), subgroup);

            if (lessons == null)
            {
                return BadRequest("Пары не найдены");
            }
            return Ok(lessons);
        }

        private int FindPage(string group)
        {
            Cell needCell;
            for (int i = 1; i <= 4; i++)
            {
                var wb = GetWB();
                WorkSheet ws = wb.GetWorkSheet(i.ToString());
                needCell = ws["B1:BH1"].FirstOrDefault(g => g.Text.Contains(group.ToString()));
                if (needCell != null)
                {
                    return i;
                }
            }
            return -1;
        }

        private static List<string> FilterSubGroupLab(int? subgroup, List<string> lessons)
		{
			if (subgroup != null)
			{
				lessons = lessons.Where(l => l.Contains($"гр.{subgroup}")).ToList();
			}

			return lessons;
		}

        private string GetGroupDescription(string inputString, string subGroup, out int position)
        {
            // Разбиваем строку по символу '/'
            string[] parts = inputString.Split('/');

            if (parts.Length >= 2)
            {
                var res = parts.Where(s => s.Contains($"гр.{subGroup}")).First();
                position = res == parts[0] ? 0: 1;
                return res;
            }
            position = 0;
            return inputString;
        }

        private string GetSplitItemFromPos(string inputString, int pos)
        {
            // Разбиваем строку по символу '/'
            if (inputString.Contains('/'))
            {
                string[] parts = inputString.Split('/');
                return parts[pos];
            }
            return inputString;
        }
    }
}
