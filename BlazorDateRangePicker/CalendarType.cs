/**
* author: Sergey Zaikin zaikinsr@yandex.ru
* copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/

using System;
using System.Collections.Generic;

namespace BlazorDateRangePicker
{
    public class CalendarType
    {
        private int DaysInMonth => DateTime.DaysInMonth(Month.Year, Month.Month);
        private int LastMonth => FirstDay.AddMonths(-1).Month;
        private int LastYear => FirstDay.AddMonths(-1).Year;
        private int DaysInLastMonth => DateTime.DaysInMonth(LastYear, LastMonth);
        private DayOfWeek DayOfWeek => FirstDay.DayOfWeek;
        internal DayOfWeek FirstDayOfWeek { get; set; }

        internal SideType Side { get; set; }
        internal DateTimeOffset? MinDate { get; set; }
        internal DateTimeOffset? MaxDate { get; set; }

        internal DateTimeOffset FirstDay => new DateTime(Month.Year, Month.Month, 1);
        internal DateTimeOffset LastDay => new DateTime(Month.Year, Month.Month, DaysInMonth);

        private DateTimeOffset month = DateTime.Today;
        public DateTimeOffset Month
        {
            get { return month; }
            set
            {
                var prevValue = month;
                month = value;
                if (prevValue.Month != value.Month
                    || prevValue.Year != value.Year
                    || Calendar == null)
                {
                    CalculateCalendar();
                }
            }
        }

        internal List<List<DateTimeOffset>> Calendar { get; set; }

        internal CalendarType(DayOfWeek firstDayOfWeek)
        {
            FirstDayOfWeek = firstDayOfWeek;
        }

        private void CalculateCalendar()
        {
            var calendar = new List<List<DateTimeOffset>>();
            for (var i = 0; i < 6; i++)
            {
                calendar.Add(new List<DateTimeOffset>());
            }

            var startDay = DaysInLastMonth - (int)DayOfWeek + (int)FirstDayOfWeek + 1;
            if (startDay > DaysInLastMonth)
            {
                startDay -= 7;
            }
            if (DayOfWeek == FirstDayOfWeek)
            {
                startDay = DaysInLastMonth - 6;
            }

            var curDate = new DateTime(LastYear, LastMonth, startDay, 12, Month.Minute, Month.Second);

            int col = 0, row = 0;
            for (var i = 0; i < 42; i++, col++, curDate = curDate.AddDays(1))
            {
                if (i > 0 && col % 7 == 0)
                {
                    col = 0;
                    row++;
                }
                calendar[row].Add(new DateTime(curDate.Year, curDate.Month, curDate.Day, Month.Hour, Month.Minute, Month.Second));

                if (MinDate.HasValue && calendar[row][col].Date == MinDate.Value.Date && calendar[row][col] < MinDate && Side == SideType.Left)
                {
                    calendar[row][col] = MinDate.Value;
                }

                if (MaxDate.HasValue && calendar[row][col].Date == MaxDate.Value.Date && calendar[row][col] > MaxDate && Side == SideType.Right)
                {
                    calendar[row][col] = MaxDate.Value;
                }
            }
            Calendar = calendar;
        }

        internal List<DateTimeOffset> this[int index]
        {
            get
            {
                return Calendar[index];
            }
            set
            {
                Calendar[index] = value;
            }
        }
    }
}
