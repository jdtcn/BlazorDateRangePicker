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
    public partial class Calendar : ComponentBase
    {

        [CascadingParameter] public DateRangePicker Picker { get; set; }
        [Parameter] public CalendarType CalendarData { get; set; }
        [Parameter] public List<string> CustomDayNames { get; set; }

        [Parameter] public SideType? Side { get; set; }
        [Parameter] public EventCallback<DateTimeOffset> OnMonthChanged { get; set; }
        [Parameter] public EventCallback<DateTimeOffset> OnClickDate { get; set; }
        [Parameter] public EventCallback<DateTimeOffset> OnHoverDate { get; set; }

        private int MinYear => Picker.MinDate?.Year ?? 1950;
        private int MaxYear => Picker.MaxDate?.Year ?? DateTime.Now.AddYears(50).Year;

        private int CurrentMonth
        {
            get => CalendarData.Month.Month;
            set => MonthSelected(value);
        }

        private int CurrentYear
        {
            get => CalendarData.Month.Year;
            set => YearSelected(value);
        }

        private bool PrevBtnVisible =>
            (!Picker.MinDate.HasValue || Picker.MinDate < CalendarData.FirstDay)
            && CalendarData.FirstDay.Date > DateTime.MinValue.Date
            && (Picker.LinkedCalendars != true || Side == SideType.Left);
        private bool NextBtnVisible =>
            (!Picker.MaxDate.HasValue || Picker.MaxDate > CalendarData.LastDay)
            && CalendarData.LastDay.Date < DateTime.MaxValue.Date
            && (Picker.LinkedCalendars != true || Side == SideType.Right || Picker.ShowOnlyOneCalendar == true);

        private List<string> DayNames => GetDayNames();

        private List<string> GetDayNames()
        {
            if (CustomDayNames?.Count == 7) return CustomDayNames;

            var dayNames = Picker.Culture.DateTimeFormat.ShortestDayNames.ToList();
            var firstDayNumber = (int)Picker.FirstDayOfWeek;
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

        private Task PreviousMonth(bool enabled)
        {
            if (!enabled) return Task.CompletedTask;
            return OnMonthChanged.InvokeAsync(CalendarData.Month.Subtract(CalendarData.Month.Offset).ToOffset(TimeSpan.Zero).AddMonths(-1));
        }

        private Task NextMonth(bool enabled)
        {
            if (!enabled) return Task.CompletedTask;
            return OnMonthChanged.InvokeAsync(CalendarData.Month.Subtract(CalendarData.Month.Offset).ToOffset(TimeSpan.Zero).AddMonths(1));
        }

        private Task MonthSelected(int month)
        {
            var d = CalendarData.Month;
            return OnMonthChanged.InvokeAsync(new DateTime(d.Year, month, 1, 12, 0, 0));
        }

        private Task YearSelected(int year)
        {
            var d = CalendarData.Month;
            var newMonth = new DateTimeOffset(year, d.Month, 1, 12, 0, 0, d.Offset);
            if (newMonth > Picker.MaxDate) newMonth = Picker.MaxDate.Value;
            else if (newMonth < Picker.MinDate) newMonth = Picker.MinDate.Value;
            return OnMonthChanged.InvokeAsync(newMonth);
        }

        private Task ClickDate(CalendarItem dt)
        {
            if (dt.Disabled) return Task.CompletedTask;
            return OnClickDate.InvokeAsync(dt.Day);
        }

        private Task OnMouseOverDate(DateTimeOffset date)
        {
            if (Picker.HoverDate != date)
            {
                return OnHoverDate.InvokeAsync(date);
            }
            return Task.CompletedTask;
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
    }
}
