/**
* @author: Sergey Zaikin zaikinsr@yandex.ru
* @copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* @license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/

using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace BlazorDateRangePicker
{
    public partial class Calendar
    {

        [CascadingParameter] public DateRangePicker Picker { get; set; }
        [Parameter] public CalendarType CalendarData { get; set; }

        [Parameter] public SideType? Side { get; set; }
        [Parameter] public EventCallback<DateTimeOffset> OnMonthChanged { get; set; }
        [Parameter] public EventCallback<DateTimeOffset> OnClickDate { get; set; }
        [Parameter] public EventCallback<DateTimeOffset> OnHoverDate { get; set; }

        private int MinYear => Picker.MinDate?.Year ?? 1900;
        private int MaxYear => Picker.MaxDate?.Year ?? DateTime.Now.AddYears(100).Year;
        private DateTimeOffset? MinDate { get => Side == SideType.Left ? Picker.MinDate : Picker.StartDate; }
        private DateTimeOffset? MaxDate
        {
            get
            {
                if (Picker.MaxSpan.HasValue && Picker.StartDate.HasValue && !Picker.EndDate.HasValue)
                {
                    var maxLimit = Picker.StartDate.Value.Add(Picker.MaxSpan.Value);
                    if (!Picker.MaxDate.HasValue || maxLimit < MaxDate)
                    {
                        return maxLimit;
                    }
                }
                return Picker.MaxDate;
            }
        }

        private bool PrevBtnVisible => 
            (!MinDate.HasValue || MinDate<CalendarData.FirstDay) 
            && (Picker.LinkedCalendars != true || Side == SideType.Left);
        private bool NextBtnVisible => 
            (!MaxDate.HasValue || MaxDate > CalendarData.LastDay) 
            && (Picker.LinkedCalendars != true || Side == SideType.Right || Picker.SingleDatePicker == true);

        private List<string> DayNames { get; set; } = new List<string>();

        private List<string> GetDayNames()
        {
            var dayNames = Picker.Culture.DateTimeFormat.ShortestDayNames.ToList();
            var firstDayNumber = (int)CalendarData.FirstDayOfWeek;
            if (firstDayNumber > 0)
            {
                for (int i = 0; i < firstDayNumber; i++)
                {
                    var item = dayNames[0];
                    dayNames.Insert(dayNames.Count, item);
                    dayNames.RemoveAt(0);
                }
            }
            return dayNames;
        }

        protected override void OnInitialized()
        {
            DayNames = GetDayNames();
            base.OnInitialized();
        }

        public bool IsDayDisabled(DateTimeOffset date)
        {
            if (Picker.DaysEnabledFunction != null)
            {
                return !Picker.DaysEnabledFunction(date);
            }
            else
            {
                return false;
            }
        }

        public string GetCustomDateClass(DateTimeOffset date)
        {
            if (Picker.CustomDateFunction == null) return string.Empty;

            var customFunctionResult = Picker.CustomDateFunction(date);
            return customFunctionResult switch
            {
                string className => className ?? string.Empty,
                bool addName => addName ? Picker.CustomDateClass ?? string.Empty : string.Empty,
                _ => string.Empty
            };
        }

        private Task PreviousMonth()
        {
            return OnMonthChanged.InvokeAsync(CalendarData.Month.AddMonths(-1));
        }

        private Task NextMonth()
        {
            return OnMonthChanged.InvokeAsync(CalendarData.Month.AddMonths(1));
        }

        private Task MonthSelected(ChangeEventArgs e)
        {
            var month = int.Parse(e.Value.ToString());
            var d = CalendarData.Month;
            return OnMonthChanged.InvokeAsync(new DateTime(d.Year, month, d.Day, d.Hour, d.Minute, d.Second));
        }

        private Task YearSelected(ChangeEventArgs e)
        {
            var year = int.Parse(e.Value.ToString());
            var d = CalendarData.Month;
            return OnMonthChanged.InvokeAsync(new DateTime(year, d.Month, d.Day, d.Hour, d.Minute, d.Second));
        }

        private Task ClickDate(bool disabled, DateTimeOffset date)
        {
            if (disabled)
            {
                return Task.CompletedTask;
            }
            return OnClickDate.InvokeAsync(date);
        }

        private Task OnMouseOverDate(DateTimeOffset date)
        {
            if (Picker.HoverDate != date)
            {
                return OnHoverDate.InvokeAsync(date);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private int GetWeekOfYear(DateTime time)
        {
            var weekRule = Picker.Culture.DateTimeFormat.CalendarWeekRule;
            var firstDayOfWeek = CalendarData.FirstDayOfWeek;
            return DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(time, weekRule, firstDayOfWeek);
        }

        private int GetIso8601WeekOfYear(DateTime time)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        private bool GetCellStateAndClasses(DateTimeOffset dt, int month, out string classNames)
        {
            var classes = new List<string>();
            var disabled = false;

            // Highlight today's date
            if (dt.Date == DateTime.Today)
            { classes.Add("today"); }

            // Highlight weekends
            if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
            { classes.Add("weekend"); }

            // Grey out the dates in other months displayed at beginning and end of this calendar
            if (dt.Month != month)
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
            if (MaxDate.HasValue && dt > MaxDate)
            {
                classes.Add("off");
                classes.Add("disabled");
                disabled = true;
            }

            // Don't allow selection of date if a custom function decides it's invalid
            if (this.IsDayDisabled(dt))
            {
                classes.Add("off");
                classes.Add("disabled");
                disabled = true;
            }

            // Highlight the currently selected start date
            if (dt.ToString("yyyy-MM-dd") == Picker.StartDate?.ToString("yyyy-MM-dd"))
            {
                classes.Add("active");
                classes.Add("start-date");
            }

            // Highlight the currently selected end date
            if (Picker.EndDate != null && dt.ToString("yyyy-MM-dd") == Picker.EndDate?.ToString("yyyy-MM-dd"))
            {
                classes.Add("active");
                classes.Add("end-date");
            }

            // Highlight dates in-between the selected dates
            if (Picker.EndDate != null && dt > Picker.StartDate && dt < Picker.EndDate)
            {
                classes.Add("in-range");
            }

            // Apply custom classes for this date
            if (Picker.CustomDateFunction != null)
            {
                classes.Add(GetCustomDateClass(dt));
            }

            // Highlight dates in-between the selected dates when hover
            if ((dt > Picker.StartDate && dt < Picker.HoverDate) || dt.Date == Picker.HoverDate?.Date)
            {
                classes.Add("in-range");
            }

            if (!disabled)
            {
                classes.Add("available");
            }

            classNames = string.Join(" ", classes);
            return disabled;
        }
    }
}
