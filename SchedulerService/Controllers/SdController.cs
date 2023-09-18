using IronXL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace SchedulerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SdController : ControllerBase
    {
        private readonly HttpClient httpClient;

        public SdController()
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

        [HttpGet("StudGet")]
        public IActionResult StudGet([FromQuery] int group, [FromQuery] int kurs)
        {
            //Load Excel file
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", $"sh18.09.2023.xls");
            WorkBook wb = WorkBook.Load(path);

            WorkSheet ws = wb.GetWorkSheet(kurs.ToString());

            var lessons = new List<string>();

            var needCell = ws["B1:BH1"].FirstOrDefault(g => g.Text.Contains(group.ToString()));
            if (needCell == null)
            {
                return BadRequest(group + " не найден");
            }

            var rowNum = Regex.Match(needCell.Location, @"\d+").Value;
            var column = Regex.Match(needCell.Location, @"[A-Za-z]+").Value;
            string startPos = column + "" + rowNum;

            string endPos = column + "" + "16";

            lessons = ws[$"{startPos}:{endPos}"].Where(s => !string.IsNullOrEmpty(s.Text)).Select(c => c.Text).ToList();

            return Ok(lessons);
        }

        [HttpGet("TeachGet")]
        public IActionResult TeachGet([FromQuery] string name)
        {
            //Load Excel file
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", $"sh18.09.2023.xls");
            WorkBook wb = WorkBook.Load(path);

            WorkSheet ws = wb.GetWorkSheet("4");

            var lessons = new List<string>();

            foreach (var cell in ws["AH3:AH16"])
            {
                if (string.IsNullOrEmpty(cell.Text) == false)
                {
                    Console.WriteLine("value is: {0}", cell.Text);
                    lessons.Add(cell.Text);
                }
            }

            return Ok(lessons);
        }
    }
}
