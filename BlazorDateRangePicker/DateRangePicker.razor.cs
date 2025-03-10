/**
* @author: Sergey Zaikin zaikinsr@yandex.ru
* @copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* @license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public delegate (bool startDateParsed, bool endDateParsed) DateParsingDelegate(string value, out DateTimeOffset startDate, out DateTimeOffset endDate);

        /// <summary>
        /// Attach a named properties config object to this instance of datepicker
        /// </summary>
        [Parameter]
        public string Config { get; set; }

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
        /// Picker popup visibility. Use Open() instead.
        /// </summary>
        [Parameter]
        public bool Visible { get; set; }

        public DateTimeOffset? TStartDate { get; set; }

        [Parameter]
        public EventCallback<DateTimeOffset?> StartDateChanged { set; get; }

        public DateTimeOffset? TEndDate { get; set; }

        [Parameter]
        public EventCallback<DateTimeOffset?> EndDateChanged { set; get; }
        
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

        private DateParsingDelegate ParseFunction => CustomParseFunction ?? TryParseDates;

        public CalendarType LeftCalendar { get; set; }
        public CalendarType RightCalendar { get; set; }

        public string ChosenLabel { get; private set; }
        internal bool CalendarsVisible { get; set; }
        internal bool Loading { get; set; }
        public DateTimeOffset? HoverDate { get; set; }

        private string EditText { get; set; }
        private TimeSpan StartTime { get; set; }
        private TimeSpan EndTime { get; set; }
        public bool Render { get; set; }

        private Task<IJSObjectReference> _module;
        private readonly string ImportPath = $"./_content/BlazorDateRangePicker/clickAndPositionHandler.js?v=" +
            typeof(DateRangePicker).Assembly.GetName().Version.ToString();

        private Task<IJSObjectReference> Module => _module ??= JSRuntime.InvokeAsync<IJSObjectReference>("import", ImportPath).AsTask();

        protected override void OnInitialized()
        {
            var configs = ServiceProvider.GetServices<DateRangePickerConfig>();
            var config = configs?.FirstOrDefault();
            if (!string.IsNullOrEmpty(Config) && configs.Any(c => c.Name == Config))
            {
                config = configs.First(c => c.Name == Config);
            }

            config ??= new DateRangePickerConfig();
            config.CopyProperties(this);

            ConfigAttributes = config.Attributes;

            if (string.IsNullOrEmpty(DateFormat))
            {
                DateFormat = Culture.DateTimeFormat.ShortDatePattern;
            }

            if (!TimePicker24Hour.HasValue)
            {
                TimePicker24Hour = !Culture.DateTimeFormat.LongTimePattern.EndsWith("tt");
            }

            StartTime = TStartDate.HasValue
                ? TStartDate.Value.TimeOfDay
                : InitialStartTime ?? TimeSpan.Zero;

            EndTime = TEndDate.HasValue
                ? TEndDate.Value.TimeOfDay
                : InitialEndTime ?? TimeSpan.FromDays(1).Add(TimeSpan.FromTicks(-1));

            if (SingleDatePicker == true && TimePicker == false && !AutoApply.HasValue) AutoApply = true;
            if (SingleDatePicker == true && !ShowOnlyOneCalendar.HasValue) ShowOnlyOneCalendar = true;

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
            if (parameters.TryGetValue(nameof(EndDate), out DateTimeOffset? endDate) && endDate != EndDate)
            {
                TEndDate = endDate;
            }

            if (parameters.TryGetValue(nameof(StartDate), out DateTimeOffset? startDate) && startDate != StartDate)
            {
                TStartDate = startDate;
                var singleDatePicker = parameters.TryGetValue(nameof(SingleDatePicker), out bool? enabled) && enabled == true;
                if (SingleDatePicker == true || singleDatePicker)
                {
                    EndDate = TStartDate;
                    TEndDate = TStartDate;
                }
            }

            if (parameters.TryGetValue(nameof(Culture), out CultureInfo culture))
            {
                if (!parameters.TryGetValue(nameof(DateFormat), out string _))
                    DateFormat = culture.DateTimeFormat.ShortDatePattern;
                if (!parameters.TryGetValue(nameof(TimePicker24Hour), out bool? _))
                    TimePicker24Hour = culture.DateTimeFormat.LongTimePattern.EndsWith("tt");
                if (!parameters.TryGetValue(nameof(FirstDayOfWeek), out DayOfWeek? _))
                    FirstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
            }

            if (LeftCalendar != null && RightCalendar != null && FirstDayOfWeek.HasValue && LeftCalendar.FirstDayOfWeek != FirstDayOfWeek)
            {
                LeftCalendar.FirstDayOfWeek = FirstDayOfWeek.Value;
                RightCalendar.FirstDayOfWeek = FirstDayOfWeek.Value;
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
                if (Ranges?.Count > 0) { result.Add("show-ranges"); }
                if (AutoApply == true) { result.Add("auto-apply"); }
                if (Loading == true) { result.Add("loading"); }
                if (Ranges == null || Ranges.Count == 0 || AlwaysShowCalendars == true || CalendarsVisible)
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

            (bool startDateParsed, bool endDateParsed) = ParseFunction(EditText, out DateTimeOffset startDate, out DateTimeOffset endDate);

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
                if (startDate.TimeOfDay == TimeSpan.Zero) startDate = SafeSetStartTime(startDate);

                TStartDate = startDate;
                TEndDate = startDate;
                EditText = null;
                return ClickApply(null);
            }
            else if (startDateParsed && endDateParsed && startDate <= endDate
                && (!minDate.HasValue || startDate >= MinDate)
                && (!maxDate.HasValue || endDate <= MaxDate))
            {
                if (startDate.TimeOfDay == TimeSpan.Zero) startDate = SafeSetStartTime(startDate);
                if (endDate.TimeOfDay == TimeSpan.Zero) endDate = SafeSetEndTime(endDate);

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

        private (bool startDateParsed, bool endDateParsed) TryParseDates(string value, out DateTimeOffset startDate, out DateTimeOffset endDate)
        {
            var dateStrings = value.Split('-').Select(s => s.Trim()).ToList();
            if (dateStrings.Count != 2)
            {
                dateStrings = [value, string.Empty];
            }

            var startDateParsed = DateTimeOffset.TryParseExact(dateStrings[0], DateFormat, Culture,
                System.Globalization.DateTimeStyles.AssumeUniversal, out startDate);
            var endDateParsed = DateTimeOffset.TryParseExact(dateStrings[1], DateFormat, Culture,
                System.Globalization.DateTimeStyles.AssumeUniversal, out endDate);

            return (startDateParsed, endDateParsed);
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

                if (TimePicker == true)
                {
                    StartTime = dates.Start.TimeOfDay;
                    EndTime = dates.End.TimeOfDay;
                }

                TStartDate = SafeSetStartTime(dates.Start);
                TEndDate = SafeSetEndTime(dates.End.Date);

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

            TStartDate = TStartDate.HasValue ? SafeSetStartTime(TStartDate.Value) : null;
            TEndDate = TEndDate.HasValue ? SafeSetEndTime(TEndDate.Value) : null;
        }

        public virtual async Task ClickDate(DateTimeOffset date)
        {
            HoverDate = null;
            if (TEndDate.HasValue || TStartDate == null || date.Date.Add(EndTime) < TStartDate)
            {
                // picking start
                TEndDate = null;
                TStartDate = SafeSetStartTime(date);
                await OnSelectionStart.InvokeAsync(TStartDate.Value);
            }
            else
            {
                // picking end
                TEndDate = SafeSetEndTime(date);
                await OnSelectionEnd.InvokeAsync(TEndDate.Value);
                if (AutoApply == true)
                {
                    await ClickApply(null);
                }
            }

            if (SingleDatePicker == true)
            {
                TStartDate = SafeSetStartTime(date);
                TEndDate = TStartDate;
                if (AutoApply == true) await ClickApply(null);
            }

            await LeftCalendar.CalculateCalendar();
            await RightCalendar.CalculateCalendar();
        }

        private DateTimeOffset SafeSetStartTime(DateTimeOffset date) => SafeSetTime(date, true);

        private DateTimeOffset SafeSetEndTime(DateTimeOffset date) => SafeSetTime(date, false);

        private DateTimeOffset SafeSetTime(DateTimeOffset date, bool startTime)
        {
            var time = TimePicker == true
                ? startTime ? StartTime : EndTime
                : startTime ? TimeSpan.Zero : TimeSpan.FromDays(1).Add(TimeSpan.FromTicks(-1));

            var isFirstDay = date.Day == 1 && date.Year == 0001 && date.Month == 1;
            var isLastDay = date.Day == 31 && date.Year == 9999 && date.Month == 12;

            if (isFirstDay)
            {
                var offset = new DateTimeOffset(date.Date.AddDays(1)).Offset;
                return date.Date.Add(time < offset ? time + offset : time);
            }
            else if (isLastDay)
            {
                var offset = new DateTimeOffset(date.Date.AddDays(-1)).Offset;
                return date.Date.Add(time - offset > TimeSpan.FromDays(1) ? time + offset : time);
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
            await Close();

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

            var module = await Module;
            await module.InvokeVoidAsync("addClickOutsideEvent", ContainerId, Id, DotNetObjectReference.Create(this));
            await module.InvokeVoidAsync("getPickerPosition", ContainerId, Id,
                Enum.GetName(typeof(DropsType), Drops).ToLower(), Enum.GetName(typeof(SideType), Opens).ToLower());

            Visible = true;
            await OnOpened.InvokeAsync(null);
            if (AutoAdjustCalendars == true || Prerender != true) await AdjustCalendars();
        }

        public virtual async Task AdjustCalendars()
        {
            Prerender = true;

            var newLeftMonth = TStartDate ?? DateTime.Today;
            var newRightMonth = LinkedCalendars == true
                ? newLeftMonth.AddMonths(1)
                : (TEndDate ?? newLeftMonth.AddMonths(1));

            if (newLeftMonth.Month == newRightMonth.Month
                && newLeftMonth.Year == newRightMonth.Year)
            {
                if (newRightMonth < DateTime.MaxValue.AddMonths(-1))
                {
                    newRightMonth = newRightMonth.AddMonths(1);
                }
            }

            var needAdjust =
                LeftCalendar?.Month.Month != newLeftMonth.Month
                || LeftCalendar?.Month.Year != newLeftMonth.Year
                || RightCalendar?.Month.Month != newRightMonth.Month
                || RightCalendar?.Month.Year != newRightMonth.Year;

            if (needAdjust)
            {
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

        public async ValueTask DisposeAsync()
        {
            if (_module != null)
            {
                var module = await _module;
                await module.DisposeAsync();
            }
        }
    }
}
