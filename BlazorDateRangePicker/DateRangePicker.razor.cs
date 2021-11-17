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
using Microsoft.AspNetCore.Components.Web;
using System.Threading.Tasks;
using System.Threading;

namespace BlazorDateRangePicker
{
    public partial class DateRangePicker : ComponentBase, IConfigurableOptions
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Guid for container id used for JSInterop 
        /// </summary>
        public string ContainerId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Id for input field used for JSInterop 
        /// </summary>
        [Parameter]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ParentId => Id;

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

        private Dictionary<string, object> ConfigAttributes { get; set; }

        protected private Dictionary<string, object> CombinedAttributes
        {
            get
            {
                var combined = new Dictionary<string, object>();
                if (ConfigAttributes != null)
                {
                    foreach (var attr in ConfigAttributes)
                    {
                        combined[attr.Key] = attr.Value;
                    }
                }

                if (Attributes != null)
                {
                    foreach (var attr in Attributes)
                    {
                        combined[attr.Key] = attr.Value;
                    }
                }
                return combined;
            }
        }

        /// <summary>
        /// Picker positioning container
        /// </summary>
        [Parameter]
        public PickerContainer Container { get; set; }

        /// <summary>
        /// Custom picker input template
        /// </summary>
        [Parameter]
        public RenderFragment<DateRangePicker> PickerTemplate { get; set; }

        /// <summary>
        /// Custom picker buttons template
        /// </summary>
        [Parameter]
        public RenderFragment<DateRangePicker> ButtonsTemplate { get; set; }

        /// <summary>
        /// Custom picker input template
        /// </summary>
        [Parameter]
        public RenderFragment<CalendarItem> DayTemplate { get; set; }

        /// <summary>
        /// Set predefined date ranges the user can select from. 
        /// Each RangeItem.Name is the label for the range, and its Start and End value representing the bounds of the range.
        /// </summary>
        [Parameter]
        public Dictionary<string, DateRange> Ranges { get; set; }

        /// <summary>
        /// Hide the apply and cancel buttons, and automatically apply a new date range as soon as two dates are clicked.
        /// </summary>
        [Parameter]
        public bool? AutoApply { get; set; }

        /// <summary>
        /// Show only a single calendar to choose one date, instead of a range picker with two calendars. 
        /// The start and end dates provided to your callback will be the same single date chosen. 
        /// </summary>
        [Parameter]
        public bool? SingleDatePicker { get; set; }

        /// <summary>
        /// Show only one calendar in the picker instead of two calendars.
        /// </summary>
        [Parameter]
        public bool? ShowOnlyOneCalendar { get; set; }

        /// <summary>
        /// Whether the picker should set dates to null when the user cleans the input
        /// </summary>
        [Parameter]
        public bool? ResetOnClear { get; set; }

        /// <summary>
        /// Normally, if you use the ranges option to specify pre-defined date ranges, 
        /// calendars for choosing a custom date range are not shown until the user clicks "Custom Range". 
        /// When this option is set to true, the calendars for choosing a custom date range are always shown instead. 
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
        public DateTimeOffset? TStartDate { get; set; }

        [Parameter]
        public EventCallback<DateTimeOffset?> StartDateChanged { set; get; }

        /// <summary>
        /// The end date of the initially selected date range
        /// </summary>
        [Parameter]
        public DateTimeOffset? EndDate { get; set; }
        public DateTimeOffset? TEndDate { get; set; }

        [Parameter]
        public EventCallback<DateTimeOffset?> EndDateChanged { set; get; }

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
        /// When enabled, the two calendars displayed will always be for two sequential months (i.e. January and February), 
        /// and both will be advanced when clicking the left or right arrows above the calendars. When disabled, 
        /// the two calendars can be individually advanced and display any month/year.
        /// </summary>
        [Parameter]
        public bool? LinkedCalendars { get; set; }

        /// <summary>
        /// Show year and month select boxes above calendars to jump to a specific month and year.
        /// </summary>
        [Parameter]
        public bool? ShowDropdowns { get; set; } = true;

        /// <summary>
        /// Displays "Custom Range" at the end of the list of predefined ranges, when the ranges option is used. 
        /// This option will be highlighted whenever the current date range selection does not match one of the predefined ranges. 
        /// Clicking it will display the calendars to select a new range.
        /// </summary>
        [Parameter]
        public bool? ShowCustomRangeLabel { get; set; } = true;

        /// <summary>
        /// Inline mode
        /// </summary>
        [Parameter]
        public bool? Inline { get; set; } = false;

        /// <summary> Specify the culture to display dates and text in. Default is CultureInfo.CurrentCulture.</summary>
        [Parameter]
        public System.Globalization.CultureInfo Culture { get; set; }

        /// <summary>The text to display on the Week number heading</summary>
        [Parameter]
        public string WeekAbbreviation { get; set; }

        /// <summary>The day of the week to start from</summary>
        [Parameter]
        public DayOfWeek? FirstDayOfWeek { get; set; }

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
        /// The minimum TimeSpan between the selected start and end dates. A value of null indicates that there is no limit.
        /// </summary>
        [Parameter]
        public TimeSpan? MinSpan { get; set; }

        /// <summary>
        /// Picker popup visibility. Use Open() instead.
        /// </summary>
        [Parameter]
        public bool Visible { get; set; }

        /// <summary>
        /// Whether the picker should close on outside click
        /// </summary>
        [Parameter]
        public bool? CloseOnOutsideClick { get; set; }

        /// <summary>
        /// Whether the picker should pick months based on selected range
        /// </summary>
        [Parameter]
        public bool? AutoAdjustCalendars { get; set; }

        /// <summary>
        /// Prerender component html before picker opening.
        /// </summary>
        [Parameter]
        public bool? Prerender { get; set; }

        /// <summary>
        /// Adds select boxes to choose times in addition to dates.
        /// </summary>
        [Parameter]
        public bool? TimePicker { get; set; }

        /// <summary>
        /// Use 24-hour instead of 12-hour times, removing the AM/PM selection.
        /// </summary>
        [Parameter]
        public bool? TimePicker24Hour { get; set; }

        /// <summary>
        /// Increment of the minutes selection list for times (i.e. 30 to allow only selection of times ending in 0 or 30).
        /// </summary>
        [Parameter]
        public int? TimePickerIncrement { get; set; }

        /// <summary>
        /// Show seconds in the timePicker.
        /// </summary>
        [Parameter]
        public bool? TimePickerSeconds { get; set; }

        /// <summary>
        /// Initial time value to show in the picker before any date selected
        /// </summary>
        [Parameter]
        public TimeSpan? InitialStartTime { get; set; }

        /// <summary>
        /// Initial time value to show in the picker before any date selected
        /// </summary>
        [Parameter]
        public TimeSpan? InitialEndTime { get; set; }

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
        /// Returns time available for selection. 
        /// </summary>
        [Parameter]
        public Func<DateTimeOffset?, Task<TimeSettings>> TimeEnabledFunction { get; set; }

        /// <summary>
        /// A function that is passed each date in the two calendars before they are displayed, 
        /// may return true or false to indicate whether that date should be available for selection or not. 
        /// </summary>
        [Parameter]
        public Func<DateTimeOffset, bool> DaysEnabledFunction { get; set; }

        /// <summary>
        /// A function that is passed each date in the two calendars before they are displayed, 
        /// may return true or false to indicate whether that date should be available for selection or not. 
        /// </summary>
        [Parameter]
        public Func<DateTimeOffset, Task<bool>> DaysEnabledFunctionAsync { get; set; }

        /// <summary>
        /// String of CSS class name to apply to calendar cell when <seealso cref="CustomDateFunction"/> returns true
        /// </summary>
        [Parameter]
        public string CustomDateClass { get; set; }

        /// <summary>
        /// A function to which each date from the calendars is passed before they are displayed, 
        /// may return a bool value indicates whether <seealso cref="CustomDateClass"/> will be added to the cell, 
        /// or a string with CSS class name to add to that date's calendar cell.
        /// </summary>
        [Parameter]
        public Func<DateTimeOffset, object> CustomDateFunction { get; set; }

        /// <summary>
        /// Triggered when the apply button is clicked, or when a predefined range is clicked
        /// </summary>
        [Parameter]
        public EventCallback<DateRange> OnRangeSelect { get; set; }

        /// <summary>
        /// Triggered when the picker reset by user
        /// </summary>
        [Parameter]
        public EventCallback OnReset { get; set; }

        /// <summary>An event that is invoked when the DatePicker is opened.</summary>
        [Parameter]
        public EventCallback OnOpened { get; set; }

        /// <summary>An event that is invoked when the DatePicker is closed.</summary>
        [Parameter]
        public EventCallback OnClosed { get; set; }

        /// <summary>An event that is invoked on backdrop click (false) or cancel button click (true).</summary>
        [Parameter]
        public EventCallback<bool> OnCancel { get; set; }

        /// <summary>An event that is invoked when left or right calendar's month changed.</summary>
        [Parameter]
        public EventCallback OnMonthChanged { get; set; }

        /// <summary>An event that is invoked when left or right calendar's month changed.</summary>
        [Parameter]
        public EventCallback<CancellationToken> OnMonthChangedAsync { get; set; }

        /// <summary>An event that is invoked when StartDate is selected</summary>
        [Parameter]
        public EventCallback<DateTimeOffset> OnSelectionStart { get; set; }

        /// <summary>An event that is invoked when EndDate is selected but before apply button is clicked</summary>
        [Parameter]
        public EventCallback<DateTimeOffset> OnSelectionEnd { get; set; }

        public CalendarType LeftCalendar { get; set; }
        public CalendarType RightCalendar { get; set; }

        internal string ChosenLabel { get; set; }
        internal bool CalendarsVisible { get; set; }
        internal bool Loading { get; set; }
        public DateTimeOffset? HoverDate { get; set; }

        private string EditText { get; set; }
        private TimeSpan StartTime { get; set; }
        private TimeSpan EndTime { get; set; }
        public bool Render { get; set; }

        protected override void OnInitialized()
        {
            var configs = ServiceProvider.GetServices<DateRangePickerConfig>();
            var config = configs?.FirstOrDefault();
            if (!string.IsNullOrEmpty(Config) && configs.Any(c => c.Name == Config))
            {
                config = configs.First(c => c.Name == Config);
            }

            if (config == null) config = new DateRangePickerConfig();
            config.CopyProperties(this);

            ConfigAttributes = config.Attributes;

            if (string.IsNullOrEmpty(DateFormat))
            {
                DateFormat = Culture.DateTimeFormat.ShortDatePattern;
            }

            if (!TimePicker24Hour.HasValue)
            {
                TimePicker24Hour = Culture.DateTimeFormat.LongTimePattern.EndsWith("tt");
            }

            StartTime = TStartDate.HasValue
                ? TStartDate.Value.TimeOfDay
                : InitialStartTime ?? TimeSpan.Zero;

            EndTime = TEndDate.HasValue
                ? TEndDate.Value.TimeOfDay
                : InitialEndTime ?? TimeSpan.FromDays(1).Add(TimeSpan.FromTicks(-1));

            if (SingleDatePicker == true && TimePicker == false && !AutoApply.HasValue) AutoApply = true;

            if (!FirstDayOfWeek.HasValue)
            {
                FirstDayOfWeek = Culture.DateTimeFormat.FirstDayOfWeek;
            }

            if (Inline == true) Prerender = true;
            Render = Prerender == true;

            LeftCalendar = new CalendarType(this, SideType.Left);
            RightCalendar = new CalendarType(this, SideType.Right);

            TStartDate = StartDate?.Date.Add(StartTime);
            TEndDate = EndDate?.Date.Add(EndTime);
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && Prerender == true) return AdjustCalendars();
            return Task.CompletedTask;
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            var paramsDict = parameters.ToDictionary();

            if (paramsDict.ContainsKey(nameof(EndDate))
                && (DateTimeOffset?)paramsDict[nameof(EndDate)] != EndDate)
            {
                TEndDate = (DateTimeOffset?)paramsDict[nameof(EndDate)];
            }

            if (paramsDict.ContainsKey(nameof(StartDate))
                && (DateTimeOffset?)paramsDict[nameof(StartDate)] != StartDate)
            {
                TStartDate = (DateTimeOffset?)paramsDict[nameof(StartDate)];
                var singleDatePicker = paramsDict.ContainsKey(nameof(SingleDatePicker))
                    && (bool)paramsDict[nameof(SingleDatePicker)] == true;
                if (SingleDatePicker == true || singleDatePicker)
                {
                    EndDate = TStartDate;
                    TEndDate = TStartDate;
                }
            }

            await base.SetParametersAsync(parameters);
        }

        protected override async Task OnParametersSetAsync()
        {
            if (TimePicker == true && AutoApply == true) AutoApply = false;
            await LeftCalendar.CalculateCalendar();
            await RightCalendar.CalculateCalendar();
        }

        public string FormattedRange
        {
            get
            {
                if (!string.IsNullOrEmpty(EditText))
                {
                    return EditText;
                }

                if (SingleDatePicker == true && TStartDate != null)
                {
                    return $"{TStartDate.Value.ToString(DateFormat, Culture)}";
                }

                if (TStartDate != null && TEndDate != null)
                {
                    return $"{TStartDate.Value.ToString(DateFormat, Culture)} - " +
                           $"{TEndDate.Value.ToString(DateFormat, Culture)}";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private string RootStyles
        {
            get
            {
                var result = new List<string>();
                if (Ranges?.Any() == true) { result.Add("show-ranges"); }
                if (AutoApply == true) { result.Add("auto-apply"); }
                if (Loading == true) { result.Add("loading"); }
                if (Ranges?.Any() != true || AlwaysShowCalendars == true || CalendarsVisible)
                {
                    result.Add("show-calendar");
                }

                if (Inline != true)
                {
                    if (Drops == DropsType.Up) { result.Add("drop-up"); }
                    result.Add($"opens{Enum.GetName(typeof(SideType), Opens).ToLower()}");
                }
                else
                {
                    result.Add("inline");
                }

                return string.Join(" ", result);
            }
        }

        public virtual Task OnTextInput(ChangeEventArgs e)
        {
            EditText = e.Value.ToString();

            var dateStrings = EditText.Split('-').Select(s => s.Trim()).ToList();
            if (dateStrings.Count != 2)
            {
                dateStrings = new List<string> { EditText, string.Empty };
            }

            var startDateParsed = DateTimeOffset.TryParseExact(dateStrings[0], DateFormat, Culture,
                System.Globalization.DateTimeStyles.AssumeUniversal, out var startDate);
            var endDateParsed = DateTimeOffset.TryParseExact(dateStrings[1], DateFormat, Culture,
                System.Globalization.DateTimeStyles.AssumeUniversal, out var endDate);

            if (startDateParsed && startDate < MinDate)
            {
                startDate = MinDate.Value.Date;
            }

            var maxDate = MaxDate;
            if (MaxSpan.HasValue)
            {
                var maxLimit = startDate.Add(MaxSpan.Value).AddTicks(-1);
                if (!maxDate.HasValue || maxLimit < maxDate)
                {
                    maxDate = maxLimit;
                }
            }

            var minDate = MinDate;
            if (MinSpan.HasValue)
            {
                var minLimit = startDate.Add(MinSpan.Value);
                if (!minDate.HasValue || minLimit > minDate)
                {
                    minDate = minLimit;
                }
            }

            if (endDateParsed)
            {
                if (endDate > maxDate) endDate = maxDate.Value.Date;
                if (endDate < minDate) endDate = minDate.Value.Date;
            }

            if (startDateParsed && SingleDatePicker == true)
            {
                if (startDate.TimeOfDay == TimeSpan.Zero) startDate = SafeSetTime(startDate, StartTime);

                TStartDate = startDate;
                TEndDate = startDate;
                EditText = null;
                return ClickApply(null);
            }
            else if (startDateParsed && endDateParsed && startDate <= endDate
                && (!minDate.HasValue || startDate >= MinDate)
                && (!maxDate.HasValue || endDate <= MaxDate))
            {
                if (startDate.TimeOfDay == TimeSpan.Zero) startDate = SafeSetTime(startDate, StartTime);
                if (endDate.TimeOfDay == TimeSpan.Zero) endDate = SafeSetTime(endDate, EndTime);

                TStartDate = startDate;
                TEndDate = endDate;
                EditText = null;
                return ClickApply(null);
            }
            else if (string.IsNullOrEmpty(EditText) && ResetOnClear == true)
            {
                EditText = null;
                return Reset();
            }

            return Task.CompletedTask;
        }

        public void LostFocus(FocusEventArgs _)
        {
            EditText = null;
        }

        public virtual async Task Reset()
        {
            TStartDate = null;
            TEndDate = null;

            await ClickApply(null);
            await OnReset.InvokeAsync(null);
        }

        private Task ClickRange(MouseEventArgs e, string label)
        {
            ChosenLabel = label;
            if (label == CustomRangeLabel)
            {
                CalendarsVisible = true;
                return Task.CompletedTask;
            }
            else
            {
                var dates = Ranges[label];
                TStartDate = dates.Start.Add(StartTime);
                TEndDate = dates.End.Date.Add(EndTime);

                if (AlwaysShowCalendars != true)
                {
                    CalendarsVisible = false;
                }
                return ClickApply(e);
            }
        }

        private Task LeftMonthChanged(DateTimeOffset date)
        {
            var leftMonth = date;
            var rightMonth = LinkedCalendars == true
                ? date.AddMonths(1)
                : (DateTimeOffset?)null;
            return MonthChanged(leftMonth, rightMonth);
        }

        private Task RightMonthChanged(DateTimeOffset date)
        {
            var rightMonth = date;
            var leftMonth = LinkedCalendars == true
                ? date.AddMonths(-1)
                : (DateTimeOffset?)null;

            return MonthChanged(leftMonth, rightMonth);
        }

        private CancellationTokenSource RunningTaskToken;
        private async Task MonthChanged(DateTimeOffset? leftDate, DateTimeOffset? rightDate)
        {
            Loading = true;

            if (leftDate.HasValue)
            {
                await LeftCalendar.ChangeMonth(leftDate.Value);
            }
            if (rightDate.HasValue)
            {
                await RightCalendar.ChangeMonth(rightDate.Value);
            }

            if (RunningTaskToken != null)
            {
                RunningTaskToken.Cancel();
                RunningTaskToken.Dispose();
                RunningTaskToken = null;
            }

            RunningTaskToken = new CancellationTokenSource();
            var cts = RunningTaskToken;
            await OnMonthChanged.InvokeAsync(null);
            var task = OnMonthChangedAsync.InvokeAsync(RunningTaskToken.Token);
            await task;
            if (!cts.IsCancellationRequested)
            {
                Loading = false;
                StateHasChanged();
            }
        }

        private void TimeChanged(TimeSpan? start = null, TimeSpan? end = null)
        {
            StartTime = start ?? StartTime;
            EndTime = end ?? EndTime;
            TStartDate = TStartDate?.Date.Add(StartTime);
            TEndDate = TEndDate?.Date.Add(EndTime);
        }

        public virtual async Task ClickDate(DateTimeOffset date)
        {
            HoverDate = null;
            if (TEndDate.HasValue || TStartDate == null || date < TStartDate)
            {
                //picking start
                TEndDate = null;
                TStartDate = SafeSetTime(date, StartTime);
                await OnSelectionStart.InvokeAsync(TStartDate.Value);
            }
            else
            {
                // picking end
                TEndDate = SafeSetTime(date, EndTime);
                await OnSelectionEnd.InvokeAsync(TEndDate.Value);
                if (AutoApply == true)
                {
                    await ClickApply(null);
                }
            }

            if (SingleDatePicker == true)
            {
                TStartDate = SafeSetTime(date, StartTime);
                TEndDate = TStartDate;
                if (AutoApply == true) await ClickApply(null);
            }

            await LeftCalendar.CalculateCalendar();
            await RightCalendar.CalculateCalendar();
        }

        private DateTimeOffset SafeSetTime(DateTimeOffset date, TimeSpan time)
        {
            var isFirstDay = date.Day == 1 && date.Year == 0001 && date.Month == 1;
            var isLastDay = date.Day == 31 && date.Year == 9999 && date.Month == 12;
            var offset = DateTimeOffset.Now.Offset;
            var tzSign = offset > TimeSpan.Zero;

            if (isFirstDay)
            {
                return date.Date.Add(tzSign ? offset : TimeSpan.Zero).Add(time);
            }
            else if (isLastDay)
            {
                return date.Date.Add(tzSign ? TimeSpan.Zero : offset).Add(time);
            }
            else
            {
                return date.Date.Add(time);
            }
        }

        private async Task OnHoverDate(DateTimeOffset date)
        {
            if (!TEndDate.HasValue)
            {
                HoverDate = date;
                await LeftCalendar.CalculateCalendar();
                await RightCalendar.CalculateCalendar();
            }
        }

        public async Task ClickApply(MouseEventArgs e)
        {
            StartDate = TStartDate;
            await StartDateChanged.InvokeAsync(TStartDate);

            EndDate = TEndDate;
            await EndDateChanged.InvokeAsync(TEndDate);

            if (TStartDate.HasValue && TEndDate.HasValue)
            {
                await OnRangeSelect.InvokeAsync(new DateRange
                {
                    Start = TStartDate.Value,
                    End = TEndDate.Value
                });
            }

            await Close();
        }

        public async Task ClickCancel(MouseEventArgs e)
        {
            TStartDate = StartDate;
            TEndDate = EndDate;
            HoverDate = null;

            await Close();
            await OnCancel.InvokeAsync(e != null);
        }

        /// <summary>
        /// Show picker popup
        /// </summary>
        public async Task Open()
        {
            Render = true;

            await Task.Yield();

            if (Visible) return;

            StartTime = TStartDate.HasValue
                ? TStartDate.Value.TimeOfDay
                : InitialStartTime ?? TimeSpan.Zero;

            EndTime = TEndDate.HasValue
                ? TEndDate.Value.TimeOfDay
                : InitialEndTime ?? TimeSpan.FromDays(1).Add(TimeSpan.FromTicks(-1));

            var selectedRange = Ranges?.FirstOrDefault(r =>
                r.Value.Start.Date == TStartDate?.Date &&
                r.Value.End.Date == TEndDate?.Date);
            if (selectedRange?.Value != null)
            {
                ChosenLabel = selectedRange.Value.Key;
                if (AlwaysShowCalendars != true) CalendarsVisible = false;
            }
            else if (CalendarsVisible || AlwaysShowCalendars == true)
            {
                ChosenLabel = CustomRangeLabel;
                CalendarsVisible = true;
            }

            await JSRuntime.InvokeAsync<object>("clickAndPositionHandler.addClickOutsideEvent", ContainerId, Id, DotNetObjectReference.Create(this));
            await JSRuntime.InvokeAsync<object>("clickAndPositionHandler.getPickerPosition", ContainerId, Id,
                Enum.GetName(typeof(DropsType), Drops).ToLower(), Enum.GetName(typeof(SideType), Opens).ToLower());

            Visible = true;
            await OnOpened.InvokeAsync(null);
            if (AutoAdjustCalendars == true || Prerender != true) await AdjustCalendars();
        }

        public async Task AdjustCalendars()
        {
            Prerender = true;

            var newLeftMonth = TStartDate ?? DateTime.Today;
            var newRightMonth = LinkedCalendars == true
                ? newLeftMonth.AddMonths(1)
                : (TEndDate ?? newLeftMonth.AddMonths(1));

            var needAdjust =
                LeftCalendar?.Month.Month != newLeftMonth.Month
                || LeftCalendar?.Month.Year != newLeftMonth.Year
                || RightCalendar?.Month.Month != newRightMonth.Month
                || RightCalendar?.Month.Year != newRightMonth.Year;

            if (needAdjust)
            {
                if (newLeftMonth.Month == newRightMonth.Month
                    && newLeftMonth.Year == newRightMonth.Year)
                {
                    if (newRightMonth < DateTime.MaxValue.AddMonths(-1))
                    {
                        newRightMonth = newRightMonth.AddMonths(1);
                    }
                }
                await MonthChanged(newLeftMonth, newRightMonth);
            }
        }

        /// <summary>
        /// Toggle picker popup state
        /// </summary>
        public async Task Toggle()
        {
            if (Visible) await Close();
            else await Open();
        }

        /// <summary>
        /// Close picker popup
        /// </summary>
        public async Task Close()
        {
            await LeftCalendar.CalculateCalendar();
            await RightCalendar.CalculateCalendar();
            Visible = false;
            await OnClosed.InvokeAsync(null);
        }

        /// <summary>
        /// JSInvokable callback to handle outside click
        /// </summary>
        [JSInvokable]
        public virtual async Task InvokeClickOutside()
        {
            if (Visible && CloseOnOutsideClick == true)
            {
                await ClickCancel(null);
                StateHasChanged();
            }
        }
    }
}
