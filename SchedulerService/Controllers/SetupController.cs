using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace SchedulerService.Controllers
{
    public class SetupController: ControllerBase
    {
        public SetupController()
        {
            
        }

        [HttpPost("LoadToServ")]
        public IActionResult LoadToServ([FromQuery] string url)
        {
            var type = @"application/vnd.ms-excel";
            var fileName = Guid.NewGuid() + ".xlsx";

            using (WebClient client = new WebClient())
            {
                try
                {
                    byte[] fileData = client.DownloadData(url);
                    var stream = new MemoryStream(fileData);
                    var file = new FormFile(stream, 0, stream.Length, "sh", fileName);

                    // Проверяем, что файл существует и не пустой
                    if (file != null && file.Length > 0)
                    {
                        // Определяем путь сохранения файла
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", fileName);

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

        /// <summary>
        /// Установка типа недели для первого сенятбря (глобальная настройка)
        /// </summary>
        /// <param name="isTop"></param>
        /// <returns></returns>
        [HttpPost("WTfs")]
        public IActionResult WeekTypeForFirstSeptember([FromQuery] bool isTop)
        {
            var wt = isTop ? "top" : "down";
            System.Environment.SetEnvironmentVariable("WT", wt);
            return Ok("WeekType is " + wt);
        }
    }
}
