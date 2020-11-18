Date Range Picker for [Blazor](https://blazor.net/)
=====================

[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/BlazorDateRangePicker.svg)](https://www.nuget.org/packages/BlazorDateRangePicker/)

![https://github.com/jdtcn/BlazorDateRangePicker](https://habrastorage.org/webt/ku/ye/jt/kuyejt2khntesrw6asg9hvwiri0.png)

## [Live Demo](https://blazordaterangepicker.azurewebsites.net/)

This date range picker component is a port of js [DateRangePicker](https://github.com/dangrossman/daterangepicker/), rewritten using C# as a Razor Component.
It creates a dropdown menu from which a user can select a range of dates.

There is no dependency with jquery, moment, or bootstrap

Features include limiting the selectable date range, localizable strings and date formats,
a single date picker mode, and predefined date ranges.

JS Interop is used for popup positioning and outside click handling. With future releases of ASP.NET Core Blazor it will be possible without js.

## Get Started

Download library from NuGet in the NuGet Package Manager, or by executing the following command in the Package Manager Console:
````shell
Install-Package BlazorDateRangePicker
````

Include these lines into your _Host.cshtml (or *index.html* for Blazor WebAssembly) file `<head></head>` section:

````html
<script src="_content/BlazorDateRangePicker/clickAndPositionHandler.js"></script>
<link rel="stylesheet" href="_content/BlazorDateRangePicker/daterangepicker.min.css" />
```` 

### Use the component:

````C#
@using BlazorDateRangePicker

<DateRangePicker/>
````
Gives you:
````HTML
<input type="text"/>
````
### Tag attributes will be passed to input field:

````C#
@using BlazorDateRangePicker

<DateRangePicker class="form-control form-control-sm" placeholder="Select dates..." />
````
Gives you:
````HTML
<input type="text" class="form-control form-control-sm" placeholder="Select dates..."/>
````

### Setting properties:
````C#
@using BlazorDateRangePicker

<DateRangePicker MinDate="DateTimeOffset.Now.AddYears(-10)" MaxDate="DateTimeOffset.Now" />
````

### Two-way data binding:
````C#
@using BlazorDateRangePicker

<DateRangePicker @bind-StartDate="StartDate" @bind-EndDate="EndDate" />

@code {
    DateTimeOffset? StartDate { get; set; } = DateTime.Today.AddMonths(-1);
    DateTimeOffset? EndDate { get; set; } = DateTime.Today.AddDays(1).AddTicks(-1);
}
````

### Handle selection event:
````C#
@using BlazorDateRangePicker

<DateRangePicker OnRangeSelect="OnRangeSelect" />

@code {
    public void OnRangeSelect(DateRange range)
    {
        //Use range.Start and range.End here
    }
}
````

### More complex usage:
Using custom markup for picker.
````C#
@using BlazorDateRangePicker

<DateRangePicker Culture="@(System.Globalization.CultureInfo.GetCultureInfo("en-US"))">
    <PickerTemplate>
        <div id="@context.Id" @onclick="context.Toggle" style="background: #fff; cursor: pointer; padding: 5px 10px; width: 250px; border: 1px solid #ccc;">
            <i class="oi oi-calendar"></i>&nbsp;
            <span>@context.FormattedRange @(string.IsNullOrEmpty(context.FormattedRange) ? "Choose dates..." : "")</span>
            <i class="oi oi-chevron-bottom float-right"></i>
        </div>
    </PickerTemplate>
</DateRangePicker>
````
Set id="@context.Id" for outside click handling to root element.

Custom buttons:

````C#
<DateRangePicker @bind-StartDate="StartDate" @bind-EndDate="EndDate">
    <ButtonsTemplate>
        <button class="cancelBtn btn btn-sm btn-default" 
            @onclick="@context.ClickCancel" type="button">Cancel</button>
        <button class="cancelBtn btn btn-sm btn-default" 
            @onclick="@(e => ResetClick(e, context))" type="button">Reset</button>
        <button class="applyBtn btn btn-sm btn-primary" @onclick="@context.ClickApply"
            disabled="@(context.TStartDate == null || context.TEndDate == null)" 
            type="button">Apply</button>
    </ButtonsTemplate>
</DateRangePicker>

@code {
    DateTimeOffset? StartDate { get; set; }
    DateTimeOffset? EndDate { get; set; }

    void ResetClick(MouseEventArgs e, DateRangePicker picker)
    {
        StartDate = null;
        EndDate = null;
        // Close the picker
        picker.Close();
        // Fire OnRangeSelectEvent
        picker.OnRangeSelect.InvokeAsync(new DateRange());
    }
}
````

Use Picker.TStartDate and Picker.TEndDate properties to get current picker state before a user clicks the 'apply' button.

### One configuration for all pickers

````C#
#Startup.cs

using BlazorDateRangePicker;

//ConfigureServices
services.AddDateRangePicker(config =>
{
    config.Attributes = new Dictionary<string, object>
    {
        { "class", "form-control form-control-sm" }
    };
});
````
It's possible to create multiple named config instances and bind it to picker with "Config" property.

````C#
services.AddDateRangePicker(config => ..., configName: "CustomConfig");

<DateRangePicker Config="CustomConfig" />
````

## Properties

| Name | Type | DefaultValue |  Description |
|------|------|--------------|--------------|
|StartDate|DateTimeOffset?|null|The beginning date of the initially selected date range.|
|EndDate|DateTimeOffset?|null|The end date of the initially selected date range.|
|MinDate|DateTimeOffset?|null|The earliest date a user may select.|
|MaxDate|DateTimeOffset?|null|The latest date a user may select.|
|MinSpan|TimeSpan?|null|The minimum span between the selected start and end dates.|
|MaxSpan|TimeSpan?|null|The maximum span between the selected start and end dates.|
|ShowDropdowns|bool|true|Show year and month select boxes above calendars to jump to a specific month and year.|
|ShowWeekNumbers|bool|false|Show localized week numbers at the start of each week on the calendars.|
|ShowISOWeekNumbers|bool|false|Show ISO week numbers at the start of each week on the calendars.|
|Ranges|Dictionary<string, DateRange>|null|Set predefined date ranges the user can select from. Each key is the label for the range.|
|ShowCustomRangeLabel|bool|true|Displays "Custom Range" at the end of the list of predefined ranges, when the ranges option is used. This option will be highlighted whenever the current date range selection does not match one of the predefined ranges. Clicking it will display the calendars to select a new range.|
|AlwaysShowCalendars|bool|false|Normally, if you use the ranges option to specify pre-defined date ranges, calendars for choosing a custom date range are not shown until the user clicks "Custom Range". When this option is set to true, the calendars for choosing a custom date range are always shown instead.|
|Opens|SideType enum: Left/Right/Center|Right|Whether the picker appears aligned to the left, to the right, or centered under the HTML element it's attached to.|
|Drops|DropsType enum: Down/Up|Down|Whether the picker appears below (default) or above the HTML element it's attached to.|
|ButtonClasses|string|btn btn-sm|CSS class names that will be added to both the apply and cancel buttons.|
|ApplyButtonClasses|string|btn-primary|CSS class names that will be added only to the apply button.|
|CancelButtonClasses|string|btn-default|CSS class names that will be added only to the cancel button.|
|Culture|CultureInfo|CultureInfo.CurrentCulture|Allows you to provide localized strings for buttons and labels, customize the date format, and change the first day of week for the calendars.|
|SingleDatePicker|bool|false|Show only a single calendar to choose one date, instead of a range picker with two calendars. The start and end dates provided to your callback will be the same single date chosen.|
|AutoApply|bool|false|Hide the apply and cancel buttons, and automatically apply a new date range as soon as two dates are clicked.|
|LinkedCalendars|bool|false|When enabled, the two calendars displayed will always be for two sequential months (i.e. January and February), and both will be advanced when clicking the left or right arrows above the calendars. When disabled, the two calendars can be individually advanced and display any month/year.|
|DaysEnabledFunction|Func<DateTimeOffset, bool>|_ => true|A function that is passed each date in the two calendars before they are displayed, and may return true or false to indicate whether that date should be available for selection or not.|
|DaysEnabledFunctionAsync|Func< DateTimeOffset, Task< bool>>|_ => true|Same as DaysEnabledFunction but with async support.|
|CustomDateFunction|Func<DateTimeOffset, object>|_ => true|A function to which each date from the calendars is passed before they are displayed, may return a bool value indicates whether the string will be added to the cell, or a string with CSS class name to add to that date's calendar cell. May return string, bool, Task<string>, Task<bool>|
|CustomDateClass|string|string.Empty|String of CSS class name to apply to that custom date's calendar cell.|
|ApplyLabel|string|"Apply"|Apply button text.|
|CancelLabel|string|"Cancel"|Cancel button text.|
|CustomRangeLabel|string|"Custom range"|Custom range label at the end of the list of predefined ranges.|
|Config|string|null|Name of the named configuration to use with this picker instance.|
|ShowOnlyOneCalendar|bool|false|Show only one calendar in the picker instead of two calendars.|
|CloseOnOutsideClick|bool|true|Whether the picker should close on outside click.|
|AutoAdjustCalendars|bool|true|Whether the picker should pick the months based on selected range.|
|PickerTemplate|RenderFragment<DateRangePicker>|null|Custom input field template|
|ButtonsTemplate|RenderFragment<DateRangePicker>|null|Custom picker buttons template|
|DayTemplate|RenderFragment<CalendarItem>|null|Custom day cell template|
|Inline|bool|false|Inline mode if true.|
|ResetOnClear|bool|true|Whether the picker should set dates to null when the user clears the input.|

## Events

| Name | Type | Description |
|------|------|-------------|
|OnRangeSelect|DateRange|Triggered when the apply button is clicked, or when a predefined range is clicked.|
|OnOpened|void|An event that is invoked when the DatePicker is opened.|
|OnClosed|void|An event that is invoked when the DatePicker is closed.|
|OnCancel|bool|An event that is invoked when user cancels the selection (`true` if by pressing "Cancel" button, `false` if by backdrop click).|
|OnReset|void|An event that is invoked when the DatePicker is cleared.|
|OnMonthChanged|void|An event that is invoked when left or right calendar's month changed.|
|OnMonthChangedAsync|Task|An event that is invoked when left or right calendar's month changed and supports CancellationToken. Use this event handler to prepare the data for CustomDateFunction.|
|OnSelectionStart|DateTimeOffset|An event that is invoked when StartDate is selected|
|OnSelectionEnd|DateTimeOffset|An event that is invoked when EndDate is selected but before "Apply" button is clicked|

## Methods

| Name |Description |
|------|------------|
|Open|Show picker popup.|
|Close|Close picker popup.|
|Toggle|Toggle picker popup state.|
|Reset|Rest picker.|
|virtual InvokeClickOutside|A JSInvocable callback to handle outside click. When inherited can be overridden to modify outside click closing behavior.|

## Types

DateRange:
````C#
public class DateRange
{
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
}
````

>Note: 
>DateRange Start and End is in local timezone. 
>
>The Start property is the start of a selected day (dateTime.Date).
>
>The End property is the end of a selected day (dateTime.Date.AddDays(1).AddTicks(-1)).

## Changelog

## 3.2.0 (netcore 3.1 and net 5)

1. Added ability to reset the picker by clearing the picker input (#42)
2. Added `ResetOnClear` property
3. Added `OnReset` event
4. Added `Reset` method

###  2.13.0 (netcore 3.1) and 3.1.0 (net 5)

1. Added ability to change input field `id` attribute (#41)

###  2.12.0 (netcore 3.1) and 3.0.0 (net 5)

1. Added new `OnSelectionEnd` event
2. Added new demo example which demonstrates how to override day click handlers
3. Exposed some internals that might be useful for picker customization

### 2.11.0

1. Fix month/year select box issue (#34, #35)

### 2.10.0

1. Add `DayTemplate` property to customize picker day cell

2. Demo applications refactored and updated with new examples 

### 2.9.0

1. Fix issue with two-way dates binding (#32)
2. Fix issue with date range label selection
3. Fix issue with single date selection mode
4. `OnSelectionStart` event now returns selected start date

### 2.8.0

1. Add OnSelectionStart event (#29)
2. Add MinSpan property (#29)

### 2.7.0

1. Breaking change! CustomDateFunction changed from Func<DateTimeOffset, bool> to Func<DateTimeOffset, object> so that it can return string, bool, Task<string>, Task<bool>.
2. OnMonthChangedAsync event added to support data loading indication.
3. Fixed issue with compilerconfig.json file (#27).

### 2.6.0

1. Add inline mode (see `Inline` property, and last example in demo application) (#20)

### 2.5.0

1. Add `OnMonthChanged` event (#19)

### 2.4.0

1. Add `ButtonsTemplate` property to make custom picker buttons possible (#17)

### 2.3.0

1. Fix an issue with month selection in calendars (#14).
2. Add AutoAdjustCalendars property.
3. Expose LeftCalendar and RightCalendar DateRangePicker options (ability to select the months manually).
4. Fix an issue with FirstDayOfWeek property when the first day is not sunday or monday.

### 2.2.0

1. Fixed performance issue with js outside click handler.

### 2.1.0

1. OnCancel event added.

### 2.0.0

1. Updated to support .NET Core 3.1.0 projects
2. Now in Blazor WebAssembly we need to add library static assets manually

In .NET Core 3.0.0 projects you should stay on 1.\*.\* version
 
## License

The MIT License (MIT)

Copyright (c) 2019-2020 Sergey Zaikin

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.