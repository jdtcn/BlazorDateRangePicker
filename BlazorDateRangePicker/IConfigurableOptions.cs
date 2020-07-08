/**
* @author: Sergey Zaikin zaikinsr@yandex.ru
* @copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* @license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace BlazorDateRangePicker
{
    internal interface IConfigurableOptions
    {
        bool? AlwaysShowCalendars { get; set; }
        string ApplyButtonClasses { get; set; }
        string ApplyLabel { get; set; }
        Dictionary<string, object> Attributes { get; set; }
        bool? AutoApply { get; set; }
        string ButtonClasses { get; set; }
        string CancelButtonClasses { get; set; }
        string CancelLabel { get; set; }
        CultureInfo Culture { get; set; }
        string CustomDateClass { get; set; }
        Func<DateTimeOffset, object> CustomDateFunction { get; set; }
        string CustomRangeLabel { get; set; }
        string DateFormat { get; set; }
        Func<DateTimeOffset, bool> DaysEnabledFunction { get; set; }
        Func<DateTimeOffset, Task<bool>> DaysEnabledFunctionAsync { get; set; }
        DropsType? Drops { get; set; }
        DateTimeOffset? EndDate { get; set; }
        DayOfWeek? FirstDayOfWeek { get; set; }
        bool? LinkedCalendars { get; set; }
        DateTimeOffset? MaxDate { get; set; }
        TimeSpan? MaxSpan { get; set; }
        TimeSpan? MinSpan { get; set; }
        DateTimeOffset? MinDate { get; set; }
        SideType? Opens { get; set; }
        Dictionary<string, DateRange> Ranges { get; set; }
        bool? ShowCustomRangeLabel { get; set; }
        bool? Inline { get; set; }
        bool? ShowDropdowns { get; set; }
        bool? ShowISOWeekNumbers { get; set; }
        bool? ShowWeekNumbers { get; set; }
        bool? SingleDatePicker { get; set; }
        DateTimeOffset? StartDate { get; set; }
        string WeekAbbreviation { get; set; }
        bool? CloseOnOutsideClick { get; set; }
        bool? AutoAdjustCalendars { get; set; }
        bool? ShowOnlyOneCalendar { get; set; }
    }
}