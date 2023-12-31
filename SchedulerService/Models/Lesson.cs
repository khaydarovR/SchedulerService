﻿namespace MVVMapp.App.Models
{
    public class Lesson
    {
        static int count = 0;
        public int Id { get; set; } = count;
        public string Name { get; set; } = "Пара название";
        public string Locate { get; set; } = "УЛК-1";
        public LessonTypeEnum LessonTypeEnum { get; set; } = LessonTypeEnum.Лабы;
        public string TeacherName { get; set; } = "X Препод";
        public DateTime StartTime { get; set; } = DateTime.Now;

        public Lesson()
        {
            count++;
        }

    }
}
