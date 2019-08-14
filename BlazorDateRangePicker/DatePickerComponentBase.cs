/**
* @author: Sergey Zaikin zaikinsr@yandex.ru
* @copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* @license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorDateRangePicker
{
    public class DatePickerComponentBase : ComponentBase, IConfigurableOptions
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Guid for container id used for JSInterop 
        /// </summary>
        public string Id { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Guid for parent id used for JSInterop 
        /// </summary>
        public string ParentId { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Attach a named properties config object to this instance of datepicker
        /// </summary>
        [Parameter]
        public string Config { get; set; }

        /// <summary>
        /// All unmatched parameters will be passed to parent element
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> Attributes { get; set; }

        /// <summary>
        /// Set predefined date ranges the user can select from. Each RangeItem.Name is the label for the range, and its Start and End value representing the bounds of the range.
        /// </summary>
        [Parameter]
        public Dictionary<string, DateRange> Ranges { get; set; }

        /// <summary>
        /// Hide the apply and cancel buttons, and automatically apply a new date range as soon as two dates are clicked.
        /// </summary>
        [Parameter]
        public bool? AutoApply { get; set; }

        /// <summary>
        /// Show only a single calendar to choose one date, instead of a range picker with two calendars. The start and end dates provided to your callback will be the same single date chosen. 
        /// </summary>
        [Parameter]
        public bool? SingleDatePicker { get; set; }

        /// <summary>
        /// Normally, if you use the ranges option to specify pre-defined date ranges, calendars for choosing a custom date range are not shown until the user clicks "Custom Range". When this option is set to true, the calendars for choosing a custom date range are always shown instead. 
        /// </summary>
        [Parameter]
        public bool? AlwaysShowCalendars { get; set; }

        /// <summary>
        /// CSS class names that will be added to both the apply and cancel buttons.
        /// </summary>
        [Parameter]
        public string ButtonClasses { get; set; }

        /// <summary>
        /// CSS class names that will be added only to the apply button. 
        /// </summary>
        [Parameter]
        public string ApplyButtonClasses { get; set; }

        /// <summary>
        /// CSS class names that will be added only to the cancel button. 
        /// </summary>
        [Parameter]
        public string CancelButtonClasses { get; set; }

        [Parameter]
        public string ApplyLabel { get; set; }

        [Parameter]
        public string CancelLabel { get; set; }

        [Parameter]
        public string CustomRangeLabel { get; set; }

        /// <summary>
        /// The beginning date of the initially selected date range.
        /// </summary>
        [Parameter]
        public DateTimeOffset? StartDate { get; set; }

        /// <summary>
        /// The end date of the initially selected date range
        /// </summary>
        [Parameter]
        public DateTimeOffset? EndDate { get; set; }

        /// <summary>
        /// Specify the format string to display dates, default is Culture.DateTimeFormat.ShortDatePattern
        /// </summary>
        [Parameter]
        public string DateFormat { get; set; }      

        /// <summary>
        /// Show localized week numbers at the start of each week on the calendars.
        /// </summary>
        [Parameter]
        public bool? ShowWeekNumbers { get; set; }

        /// <summary>
        /// Show ISO week numbers at the start of each week on the calendars.
        /// </summary>
        [Parameter]
        public bool? ShowISOWeekNumbers { get; set; }

        /// <summary>
        /// When enabled, the two calendars displayed will always be for two sequential months (i.e. January and February), and both will be advanced when clicking the left or right arrows above the calendars. When disabled, the two calendars can be individually advanced and display any month/year.
        /// </summary>
        [Parameter]
        public bool? LinkedCalendars { get; set; }

        /// <summary>
        /// Show year and month select boxes above calendars to jump to a specific month and year.
        /// </summary>
        [Parameter]
        public bool? ShowDropdowns { get; set; } = true;

        /// <summary>
        /// Displays "Custom Range" at the end of the list of predefined ranges, when the ranges option is used. This option will be highlighted whenever the current date range selection does not match one of the predefined ranges. Clicking it will display the calendars to select a new range.
        /// </summary>
        [Parameter]
        public bool? ShowCustomRangeLabel { get; set; } = true;

        /// <summary> Specify the culture to display dates and text in. Default is CultureInfo.CurrentCulture.</summary>
        [Parameter]
        public System.Globalization.CultureInfo Culture { get; set; }

        /// <summary>The text to display on the Week number heading</summary>
        [Parameter]
        public string WeekAbbreviation { get; set; }

        /// <summary>The day of the week to start from</summary>
        [Parameter]
        public DayOfWeek? FirstDayOfWeek { get; set; } = DayOfWeek.Monday;

        /// <summary>The earliest date that can be selected, inclusive. A value of null indicates that there is no minimum date.</summary>
        [Parameter]
        public DateTimeOffset? MinDate { get; set; }

        /// <summary>The latest date that can be selected, inclusive. A value of null indicates that there is no maximum date.</summary>
        [Parameter]
        public DateTimeOffset? MaxDate { get; set; }

        /// <summary>
        /// The maximum TimeSpan between the selected start and end dates. A value of null indicates that there is no limit.
        /// </summary>
        [Parameter]
        public TimeSpan? MaxSpan { get; set; }

        /// <summary>
        /// Picker popup visibility. Use Open() instead.
        /// </summary>
        [Parameter]
        public bool Visible { get; set; }

        /// <summary>
        /// Whether the picker appears aligned to the left, to the right, or centered under the HTML element it's attached to.
        /// </summary>
        [Parameter]
        public SideType? Opens { get; set; }

        /// <summary>
        /// Whether the picker appears below (default) or above the HTML element it's attached to.
        /// </summary>
        [Parameter]
        public DropsType? Drops { get; set; }

        /// <summary>
        /// A function that is passed each date in the two calendars before they are displayed, and may return true or false to indicate whether that date should be available for selection or not. 
        /// </summary>
        [Parameter]
        public Func<DateTimeOffset, bool> DaysEnabledFunction { get; set; }

        /// <summary>
        /// String of CSS class name to apply to that custom date's calendar cell <seealso cref="CustomDateFunction"/>
        /// </summary>
        [Parameter]
        public string CustomDateClass { get; set; }

        /// <summary>
        /// A function that is passed each date in the two calendars before they are displayed, and may return a string or array of CSS class names to apply to that date's calendar cell.
        /// </summary>
        [Parameter]
        public Func<DateTimeOffset, bool> CustomDateFunction { get; set; }

        /// <summary>
        /// Triggered when the apply button is clicked, or when a predefined range is clicked
        /// </summary>
        [Parameter]
        public EventCallback<DateRange> OnRangeSelect { get; set; }

        /// <summary>An event that is invoked when the DatePicker is opened.</summary>
        [Parameter]
        public EventCallback OnOpened { get; set; }

        /// <summary>An event that is invoked when the DatePicker is closed.</summary>
        [Parameter]
        public EventCallback OnClosed { get; set; }

        internal DateTimeOffset? OldStartValue { get; set; }
        internal DateTimeOffset? OldEndValue { get; set; }
        internal string ChosenLabel { get; set; }
        internal bool CalendarsVisible { get; set; }

        protected override void OnInitialized()
        {
            var markupAttributes = Attributes;

            var configs = ServiceProvider.GetServices<DateRangePickerConfig>();
            var config = configs?.FirstOrDefault();
            if (!string.IsNullOrEmpty(Config) && configs.Any(c => c.Name == Config))
            {
                config = configs.First(c => c.Name == Config);
            }

            if (config == null) config = new DateRangePickerConfig();
            config.CopyProperties(this);

            if (markupAttributes?.Any() == true)
            {
                if (Attributes == null) Attributes = new Dictionary<string, object>();
                foreach (var ma in markupAttributes)
                {
                    if (!Attributes.ContainsKey(ma.Key)) Attributes[ma.Key] = ma.Value;
                }
            }

            if (string.IsNullOrEmpty(DateFormat))
            {
                DateFormat = Culture.DateTimeFormat.ShortDatePattern;
            }

            if (SingleDatePicker == true) AutoApply = true;
            base.OnInitialized();
        }

        /// <summary>
        /// Show picker popup
        /// </summary>
        public void Open()
        {
            OldStartValue = StartDate;
            OldEndValue = EndDate;

            var selectedRange = Ranges?.FirstOrDefault(r => 
                r.Value.Start.Date == StartDate?.Date && 
                r.Value.End.Date == EndDate?.Date);
            if (selectedRange != null)
            {
                ChosenLabel = selectedRange.Value.Key;
            }
            else
            {
                ChosenLabel = CustomRangeLabel;
                ShowCalendars();
            }

            Visible = true;

            Issue11159 fixer = new Issue11159(JSRuntime);
            DotNetObjectRef<DatePickerComponentBase> reference = fixer.CreateDotNetObjectRef(this);
            JSRuntime.InvokeAsync<object>("clickAndPositionHandler.addClickOutsideEvent", Id, ParentId, reference);

            JSRuntime.InvokeAsync<object>("clickAndPositionHandler.getPickerPosition", Id, ParentId,
                Enum.GetName(typeof(DropsType), Drops).ToLower(), Enum.GetName(typeof(SideType), Opens).ToLower());

            OnOpened.InvokeAsync(null);
        }

        /// <summary>
        /// Toggle picker popup state
        /// </summary>
        public void Toggle()
        {
            if (Visible) Close();
            else Open();
        }

        /// <summary>
        /// Close picker popup
        /// </summary>
        public void Close()
        {
            Visible = false;
            StateHasChanged();
            OnClosed.InvokeAsync(null);
        }

        /// <summary>
        /// JSInvokable callback to handle outside click
        /// </summary>
        [JSInvokable]
        public void InvokeClickOutside()
        {
            Visible = false;
            StateHasChanged();
            OnClosed.InvokeAsync(null);
        }

        internal void ShowCalendars()
        {
            CalendarsVisible = true;
        }

        internal void HideCalendars()
        {
            CalendarsVisible = false;
        }
    }

    public class Issue11159
    {
        private IJSRuntime Js { get; set; }

        public Issue11159(IJSRuntime jsRuntime)
        {
            Js = jsRuntime;
        }

        private static readonly object CreateDotNetObjectRefSyncObj = new object();

        public DotNetObjectRef<T> CreateDotNetObjectRef<T>(T value) where T : class
        {
            lock (CreateDotNetObjectRefSyncObj)
            {
                JSRuntime.SetCurrentJSRuntime(Js);
                return DotNetObjectRef.Create(value);
            }
        }

        public void DisposeDotNetObjectRef<T>(DotNetObjectRef<T> value) where T : class
        {
            if (value != null)
            {
                lock (CreateDotNetObjectRefSyncObj)
                {
                    JSRuntime.SetCurrentJSRuntime(Js);
                    value.Dispose();
                }
            }
        }
    }
}