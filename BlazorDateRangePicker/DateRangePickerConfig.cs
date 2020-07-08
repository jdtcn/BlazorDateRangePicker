/**
* @author: Sergey Zaikin zaikinsr@yandex.ru
* @copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* @license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorDateRangePicker
{
    public class DateRangePickerConfig : IConfigurableOptions
    {
        public string Name { get; set; }

        public Dictionary<string, object> Attributes { get; set; }

        /// <summary>
        /// Set predefined date ranges the user can select from. Each RangeItem.Name is the label for the range, and its Start and End value representing the bounds of the range.
        /// </summary>
        public Dictionary<string, DateRange> Ranges { get; set; }

        /// <summary>
        /// Hide the apply and cancel buttons, and automatically apply a new date range as soon as two dates are clicked.
        /// </summary>
        public bool? AutoApply { get; set; }

        /// <summary>
        /// Show only a single calendar to choose one date, instead of a range picker with two calendars. The start and end dates provided to your callback will be the same single date chosen. 
        /// </summary>
        public bool? SingleDatePicker { get; set; }

        /// <summary>
        /// Show only one calendar in the picker instead of two calendars.
        /// </summary>
        public bool? ShowOnlyOneCalendar { get; set; }

        /// <summary>
        /// Normally, if you use the ranges option to specify pre-defined date ranges, calendars for choosing a custom date range are not shown until the user clicks "Custom Range". When this option is set to true, the calendars for choosing a custom date range are always shown instead. 
        /// </summary>
        public bool? AlwaysShowCalendars { get; set; }

        /// <summary>
        /// CSS class names that will be added to both the apply and cancel buttons.
        /// </summary>
        public string ButtonClasses { get; set; } = "btn btn-sm";

        /// <summary>
        /// CSS class names that will be added only to the apply button. 
        /// </summary>
        public string ApplyButtonClasses { get; set; } = "btn-primary";

        /// <summary>
        /// CSS class names that will be added only to the cancel button. 
        /// </summary>
        public string CancelButtonClasses { get; set; } = "btn-default";

        public string ApplyLabel { get; set; } = "Apply";

        public string CancelLabel { get; set; } = "Cancel";

        public string CustomRangeLabel { get; set; } = "Custom range";

        /// <summary>
        /// The beginning date of the initially selected date range.
        /// </summary>
        public DateTimeOffset? StartDate { get; set; }

        /// <summary>
        /// The end date of the initially selected date range
        /// </summary>
        public DateTimeOffset? EndDate { get; set; }

        /// <summary>
        /// Specify the format string to display dates, default is Culture.DateTimeFormat.ShortDatePattern
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Show localized week numbers at the start of each week on the calendars.
        /// </summary>
        public bool? ShowWeekNumbers { get; set; }

        /// <summary>
        /// Show ISO week numbers at the start of each week on the calendars.
        /// </summary>
        public bool? ShowISOWeekNumbers { get; set; }

        /// <summary>
        /// When enabled, the two calendars displayed will always be for two sequential months (i.e. January and February), and both will be advanced when clicking the left or right arrows above the calendars. When disabled, the two calendars can be individually advanced and display any month/year.
        /// </summary>
        public bool? LinkedCalendars { get; set; }

        /// <summary>
        /// Show year and month select boxes above calendars to jump to a specific month and year.
        /// </summary>
        public bool? ShowDropdowns { get; set; } = true;

        /// <summary>
        /// Displays "Custom Range" at the end of the list of predefined ranges, when the ranges option is used. This option will be highlighted whenever the current date range selection does not match one of the predefined ranges. Clicking it will display the calendars to select a new range.
        /// </summary>
        public bool? ShowCustomRangeLabel { get; set; } = true;

        /// <summary>
        /// Inline mode
        /// </summary>
        public bool? Inline { get; set; } = false;

        /// <summary>
        /// Whether the picker should close on outside click
        /// </summary>
        public bool? CloseOnOutsideClick { get; set; } = true;

        /// <summary>
        /// Whether the picker should pick months based on selected range
        /// </summary>
        public bool? AutoAdjustCalendars { get; set; } = true;

        /// <summary> Specify the culture to display dates and text in. Default is CultureInfo.CurrentCulture.</summary>
        public System.Globalization.CultureInfo Culture { get; set; } = System.Globalization.CultureInfo.CurrentCulture;

        /// <summary>The text to display on the Week number heading</summary>
        public string WeekAbbreviation { get; set; } = string.Empty;

        /// <summary>The day of the week to start from</summary>
        public DayOfWeek? FirstDayOfWeek { get; set; }

        /// <summary>The earliest date that can be selected, inclusive. A value of null indicates that there is no minimum date.</summary>
        public DateTimeOffset? MinDate { get; set; }

        /// <summary>The latest date that can be selected, inclusive. A value of null indicates that there is no maximum date.</summary>
        public DateTimeOffset? MaxDate { get; set; }

        /// <summary>
        /// The maximum TimeSpan between the selected start and end dates. A value of null indicates that there is no limit.
        /// </summary>
        public TimeSpan? MaxSpan { get; set; }

        /// <summary>
        /// The minimum TimeSpan between the selected start and end dates. A value of null indicates that there is no limit.
        /// </summary>
        public TimeSpan? MinSpan { get; set; }

        /// <summary>
        /// Whether the picker appears aligned to the left, to the right, or centered under the HTML element it's attached to.
        /// </summary>
        public SideType? Opens { get; set; } = SideType.Right;

        /// <summary>
        /// Whether the picker appears below (default) or above the HTML element it's attached to.
        /// </summary>
        public DropsType? Drops { get; set; } = DropsType.Down;

        /// <summary>
        /// A function that is passed each date in the two calendars before they are displayed, and may return true or false to indicate whether that date should be available for selection or not. 
        /// </summary>
        public Func<DateTimeOffset, bool> DaysEnabledFunction { get; set; }

        /// <summary>
        /// A function that is passed each date in the two calendars before they are displayed, and may return true or false to indicate whether that date should be available for selection or not. 
        /// </summary>
        public Func<DateTimeOffset, Task<bool>> DaysEnabledFunctionAsync { get; set; }

        /// <summary>
        /// String of CSS class name to apply to calendar cell when <seealso cref="CustomDateFunction"/> returns true
        /// </summary>
        public string CustomDateClass { get; set; }

        /// <summary>
        /// A function to which each date from the calendars is passed before they are displayed, 
        /// may return a bool value indicates whether <seealso cref="CustomDateClass"/> will be added to the cell, 
        /// or a string with CSS class name to add to that date's calendar cell.
        /// </summary>
        public Func<DateTimeOffset, object> CustomDateFunction { get; set; } = _ => false;
    }
}
