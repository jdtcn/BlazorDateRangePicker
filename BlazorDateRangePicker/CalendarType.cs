/**
* author: Sergey Zaikin zaikinsr@yandex.ru
* copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        internal DateTimeOffset FirstDay => new DateTime(Month.Year, Month.Month, 1);
        internal DateTimeOffset LastDay => new DateTime(Month.Year, Month.Month, DaysInMonth);

        public DateTimeOffset Month { get; private set; } = DateTime.Today;

        internal List<List<CalendarItem>> Calendar { get; set; }
        private DateRangePicker Picker { get; set; }

        internal CalendarType(DateRangePicker picker)
        {
            Picker = picker;
            FirstDayOfWeek = Picker.FirstDayOfWeek.Value;
        }

        internal async Task ChangeMonth(DateTimeOffset month)
        {
            Month = month;
            await CalculateCalendar();
        }

        public async Task CalculateCalendar()
        {
            var calendar = Calendar ?? new List<List<CalendarItem>>();
            for (var i = calendar.Count; i < 6; i++)
            {
                calendar.Add(new List<CalendarItem>());
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

            var curDate = new DateTimeOffset(LastYear, LastMonth, startDay, 12, 0, 0, Month.Offset);

            int col = 0, row = 0;
            for (var i = 0; i < 42; i++, col++, curDate = curDate.AddDays(1))
            {
                if (i > 0 && col % 7 == 0)
                {
                    col = 0;
                    row++;
                }

                var day = new DateTimeOffset(curDate.Year, curDate.Month, curDate.Day, 12, 0, 0, Month.Offset);
                if (calendar[row].Count <= col)
                    calendar[row].Add(new CalendarItem { Day = day });
                else if (calendar[row][col].Day != day)
                    calendar[row][col].Day = day;

                await UpdateCellClasses(calendar[row][col]);
            }
            Calendar = calendar;
        }

        private async Task UpdateCellClasses(CalendarItem day)
        {
            var classes = new List<string>();
            var disabled = false;
            var dt = day.Day;
            // Highlight today's date
            if (dt.Date == DateTime.Today)
            { classes.Add("today"); }

            // Highlight weekends
            if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
            { classes.Add("weekend"); }

            // Grey out the dates in other months displayed at beginning and end of this calendar
            if (dt.Month != Month.Month)
            {
                classes.Add("off");
                classes.Add("ends");
            }
            // Don't allow selection of dates before the minimum date
            if (Picker.MinDate.HasValue && dt < Picker.MinDate)
            {
                classes.Add("off");
                classes.Add("disabled");
                disabled = true;
            }
            // Don't allow selection of dates after the maximum date
            if (Picker.MaxDate.HasValue && dt > Picker.MaxDate)
            {
                classes.Add("off");
                classes.Add("disabled");
                disabled = true;
            }

            if (Picker.MinSpan.HasValue && Picker.TStartDate.HasValue && Picker.TEndDate == null 
                && dt - Picker.TStartDate >= TimeSpan.Zero && dt - Picker.TStartDate < Picker.MinSpan)
            {
                classes.Add("disabled");
                disabled = true;
            }

            if (Picker.MaxSpan.HasValue && Picker.TStartDate.HasValue && Picker.TEndDate == null
                && dt - Picker.TStartDate > Picker.MaxSpan)
            {
                classes.Add("off");
                classes.Add("disabled");
                disabled = true;
            }

            // Don't allow selection of date if a custom function decides it's invalid
            if (await IsDayDisabled(dt))
            {
                classes.Add("disabled");
                disabled = true;
            }

            // Highlight the currently selected start date
            if (dt.ToString("yyyy-MM-dd") == Picker.TStartDate?.ToString("yyyy-MM-dd"))
            {
                classes.Add("active");
                classes.Add("start-date");
            }

            // Highlight the currently selected end date
            if (Picker.TEndDate != null && dt.ToString("yyyy-MM-dd") == Picker.TEndDate?.ToString("yyyy-MM-dd"))
            {
                classes.Add("active");
                classes.Add("end-date");
            }

            // Highlight dates in-between the selected dates
            if (Picker.TEndDate != null && dt > Picker.TStartDate && dt < Picker.TEndDate)
            {
                classes.Add("in-range");
            }

            // Apply custom classes for this date
            if (Picker.CustomDateFunction != null)
            {
                classes.Add(await GetCustomDateClass(dt));
            }

            // Highlight dates in-between the selected dates when hover
            if ((dt > Picker.TStartDate && dt < Picker.HoverDate) || dt.Date == Picker.HoverDate?.Date)
            {
                classes.Add("in-range");
            }

            if (!disabled)
            {
                classes.Add("available");
            }

            day.Disabled = disabled;
            day.ClassNames = string.Join(" ", classes.Distinct());
        }

        private async Task<bool> IsDayDisabled(DateTimeOffset date)
        {
            if (Picker.DaysEnabledFunction != null)
            {
                return !Picker.DaysEnabledFunction(date);
            }
            else if (Picker.DaysEnabledFunctionAsync != null)
            {
                return !await Picker.DaysEnabledFunctionAsync(date);
            }
            return false;
        }

        private async Task<string> GetCustomDateClass(DateTimeOffset date)
        {
            if (Picker.CustomDateFunction == null) return string.Empty;

            var customFunctionResult = Picker.CustomDateFunction(date);
            return customFunctionResult switch
            {
                string className => className ?? string.Empty,
                bool addName => addName ? Picker.CustomDateClass ?? string.Empty : string.Empty,
                Task<string> task => await task,
                Task<bool> task => await task ? Picker.CustomDateClass ?? string.Empty : string.Empty,
                Task<object> task => await task switch
                {
                    string className => className ?? string.Empty,
                    bool addName => addName ? Picker.CustomDateClass ?? string.Empty : string.Empty,
                    _ => string.Empty
                },
                _ => string.Empty
            };
        }
    }

    public class CalendarItem
    {
        public DateTimeOffset Day { get; set; }
        public Action Hover { get; set; }
        public Action Click { get; set; }
        public string ClassNames { get; set; }
        public bool Disabled { get; set; }
    }
}
