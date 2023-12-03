using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SchedulerService.Helpers;
using System.IO;
using System.Net;

namespace SchedulerService.Controllers
{
    public class SetupController: ControllerBase
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        public SetupController(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("LoadFromServ")]
        public IActionResult LoadFromServ([FromQuery] string url)
        {
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

        [HttpGet("DownloadLast")]
        public IActionResult DownloadLast()
        {
            // Get the full path of the file
            var fullPathFile = ParserHelper.GetLastChangedFilePath();

            // Check if the file exists
            if (System.IO.File.Exists(fullPathFile))
            {
                // Determine the content type based on the file extension
                var contentType = "application/octet-stream"; // You may need to set the appropriate content type for your file type

                // Get the file name from the full path
                var fileName = System.IO.Path.GetFileName(fullPathFile);

                // Return the file for download
                return File(System.IO.File.ReadAllBytes(fullPathFile), contentType, fileName);
            }
            else
            {
                // Return a not found response or handle the case when the file does not exist
                return NotFound();
            }
        }


        [HttpPost("LoadFromClient")]
        public IActionResult LoadFromClient(IFormFile formFile)
        {
            var type = @"application/vnd.ms-excel";
            var fileName = Guid.NewGuid() + ".xlsx";

            {
                try
                {
                    var stream = new MemoryStream();
                    formFile.CopyTo(stream);
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


        [HttpPost("LoadFromClient2")]
        public IActionResult LoadFromClient2([FromBody] string data)
        {
            var fileName = Guid.NewGuid() + ".xlsx";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", fileName);

            try
            {
                System.IO.File.WriteAllBytes(filePath, Convert.FromBase64String(data));
                return Content("Файл успешно сохранен.");
            }
            catch (Exception ex)
            {
                return Content("Ошибка при сохранении файла: " + ex.Message);
            }
        }

    }
}
