using MVVMapp.App.Models;

namespace SchedulerService.Helpers
{
    public class WeekTypeCalculator
    {
        public static WeekTypeEnum CalculateWeekType(DateTime currentDate, WeekTypeEnum weekTypeOnSeptember1)
        {
            // Определяем день недели для 1 сентября текущего года
            DateTime september1 = new DateTime(currentDate.Year, 9, 1);
            DayOfWeek dayOfWeekOnSeptember1 = september1.DayOfWeek;

            // Определяем разницу в днях между 1 сентября и произвольной датой
            int daysDifference = (int)(currentDate - september1).TotalDays;

            // Вычисляем, сколько недель прошло с 1 сентября и определяем тип недели
            int weeksPassed = daysDifference / 7;
            WeekTypeEnum currentWeekType = weekTypeOnSeptember1;

            for (int i = 0; i < weeksPassed; i++)
            {
                // Переключаемся между верхней и нижней неделями
                currentWeekType = currentWeekType == WeekTypeEnum.Вверхняя ? WeekTypeEnum.Нижняя : WeekTypeEnum.Вверхняя;
            }

            return currentWeekType;
        }
    }
}
