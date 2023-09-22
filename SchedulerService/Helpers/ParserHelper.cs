using IronXL;
using MVVMapp.App.Models;

namespace SchedulerService.Helpers
{
    public static class ParserHelper
    {
        public static WeekTypeEnum WeekTypeFirstSeptember = WeekTypeEnum.Нижняя;

        public static WorkBook GetWB()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", $"sh18.09.2023.xls");
            WorkBook wb = WorkBook.Load(path);
            return wb;
        }

        public static WeekTypeEnum IsNeedWeekType(DateTime date)
        {
            var res = WeekTypeCalculator.CalculateWeekType(date, WeekTypeFirstSeptember);
            return res;
        }

        public static string GetGroupDescription(string inputString, string subGroup, out int position)
        {
            // Разбиваем строку по символу '/'
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
    }
}
