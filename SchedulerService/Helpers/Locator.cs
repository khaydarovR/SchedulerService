using IronXL;
using System.Text.RegularExpressions;

namespace SchedulerService.Helpers
{
    public class Locator
    {
        private int x;
        private int y;

        public Locator(Cell cell)
        {
            var input = cell.Location;
            string column = GetColumn(input);
            string row = GetRow(input);
            x = ConvertColumnToIndex(column);
            y = int.Parse(row);
        }

        public Locator MoveRight(int count = 1)
        {
            x += count;
            return this;
        }

        public Locator MoveLeft(int count = 1)
        {
            x -= count;
            return this;
        }

        public Locator MoveUp(int count = 1)
        {
            y -= count;
            return this;
        }

        public Locator MoveDown(int count = 1)
        {
            y += count;
            return this;
        }

        private string GetColumn(string input)
        {
            return Regex.Match(input, "[A-Z]+").Value;
        }

        // Получить номер строки из входной позиции
        private string GetRow(string input)
        {
            return Regex.Match(input, @"\d+").Value;
        }

        // Преобразовать буквенное обозначение столбца в числовой индекс
        private int ConvertColumnToIndex(string column)
        {
            int columnIndex = 0;
            foreach (char c in column)
            {
                columnIndex = columnIndex * 26 + c - 'A' + 1;
            }
            return columnIndex;
        }

        // Преобразовать числовой индекс столбца в буквенное обозначение
        private string ConvertIndexToColumn(int index)
        {
            string newColumn = "";
            while (index > 0)
            {
                int remainder = (index - 1) % 26;
                newColumn = (char)('A' + remainder) + newColumn;
                index = (index - 1) / 26;
            }
            return newColumn;
        }

        // Объединить буквенное обозначение столбца и строку
        public string GetNewLocation()
        {
            return ConvertIndexToColumn(x) + y;
        }

    }
}
