using IronXL;
using MVVMapp.App.Models;

namespace SchedulerService.Helpers
{
    public static class ParserHelper
    {
        public static WorkBook GetLatestUploadedWB()
        {
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            // Получаем все файлы в папке "Uploads"
            var files = Directory.GetFiles(directoryPath);

            // Если нет файлов, возвращаем null или бросаем исключение, в зависимости от ваших требований

            if (files.Length == 0)
            {
                throw new FileNotFoundException("Нет загруженных файлов в папке.");
            }

            // Ищем последний файл по дате создания
            var latestFile = files
                .Select(filePath => new FileInfo(filePath))
                .OrderByDescending(fileInfo => fileInfo.LastWriteTime)
                .First();

            // Загружаем найденный файл
            var wb = WorkBook.Load(latestFile.FullName);

            return wb;
        }

        public static string GetLastChangedFilePath()
        {
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            // Получаем все файлы в папке "Uploads"
            var files = Directory.GetFiles(directoryPath);

            // Если нет файлов, возвращаем null или бросаем исключение, в зависимости от ваших требований

            if (files.Length == 0)
            {
                throw new FileNotFoundException("Нет загруженных файлов в папке.");
            }

            // Ищем последний файл по дате создания
            var latestFile = files
                .Select(filePath => new FileInfo(filePath))
                .OrderByDescending(fileInfo => fileInfo.LastWriteTime)
                .First();
        
            return latestFile.FullName;
        }
        

        public static WeekTypeEnum IsNeedWeekType(DateTime date)
        {
            var WeekTypeFirstSeptember = System.Environment.GetEnvironmentVariable("WT") == "top" ? WeekTypeEnum.Вверхняя : WeekTypeEnum.Нижняя;
            var res = WeekTypeCalculator.CalculateWeekType(date, WeekTypeFirstSeptember);
            return res;
        }

        public static string GetGroupDescription(string inputString, string subGroup, out int position)
        {
            // Разбиваем строку по символу '/'
            if (inputString.Contains('/'))
            {
                string[] parts = inputString.Split('/');

                if (parts.Length >= 2)
                {
                    var res = parts.Where(s => s.Contains($"гр.{subGroup}")).First();
                    position = res == parts[0] ? 0 : 1;
                    return res;
                }
                position = 0;
                return inputString;
            }
            else
            {
                position = 0;
                return inputString;
            }
            
        }

        public static string GetSplitItemFromPos(string inputString, int pos)
        {
            // Разбиваем строку по символу '/'
            if (inputString.Contains('/'))
            {
                string[] parts = inputString.Split('/');
                return parts[pos];
            }
            return inputString;
        }

        public static int FindPage(string group)
        {
            Cell needCell;
            for (int i = 1; i <= 4; i++)
            {
                var wb = GetLatestUploadedWB();
                WorkSheet ws = wb.GetWorkSheet(i.ToString());
                needCell = ws["B1:BH1"].FirstOrDefault(g => g.Text.Contains(group.ToString()));
                if (needCell != null)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
