Date Range Picker for [Blazor](https://blazor.net/)
=====================

[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/BlazorDateRangePicker.svg)](https://www.nuget.org/packages/BlazorDateRangePicker/)

![https://github.com/jdtcn/BlazorDateRangePicker](https://habrastorage.org/webt/4s/ml/el/4smlelm0wf58eyzl4sryzg6vsyi.png)

This date range picker component is a port of js [DateRangePicker](https://github.com/dangrossman/daterangepicker/), rewritten using C# as a Razor Component.
It creates a dropdown menu from which a user can select a range of dates.

There is no dependecy with jquery, moment, or bootstrap

Features include limiting the selectable date range, localizable strings and date formats,
a single date picker mode, and predefined date ranges.

JS Interop is used for popup positioning and outside click handling. With future releases of ASP.NET Core Blazor it will be possible without js.

## Get Started

Download library from NuGet in the NuGet Package Manager, or by executing the following command in the Package Manager Console:
````shell
Install-Package BlazorDateRangePicker -Pre
````

Temporarily server-side Blazor can't serve library static assets (css and js) so we need [EmbeddedBlazorContent](https://github.com/SamProf/EmbeddedBlazorContent) library for server-side Blazor.
````shell
Install-Package EmbeddedBlazorContent -Pre
````

````C#
#Startup.cs

using EmbeddedBlazorContent;

//Configure
//after app.UseStaticFiles();
app.UseEmbeddedBlazorContent(typeof(BlazorDateRangePicker.DateRangePicker).Assembly);
````

````C#
# _Host.cshtml
@using EmbeddedBlazorContent
....
<head>
....
@Html.EmbeddedBlazorContent()
...
</head>
````

Otherwise you must load *.css and *.js files from content filder into your project manually.



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

### More complex usage:
Using custom markup for picker.
````C#
@using BlazorDateRangePicker

<DateRangePicker Culture="@(System.Globalization.CultureInfo.GetCultureInfo("en-US"))">
    <PickerTemplate>
        <div id="@context.ParentId" @onclick="context.Toggle" style="background: #fff; cursor: pointer; padding: 5px 10px; width: 250px; border: 1px solid #ccc;">
            <i class="oi oi-calendar"></i>&nbsp;
            <span>@context.FormattedRange @(string.IsNullOrEmpty(context.FormattedRange) ? "Choose dates..." : "")</span>
            <i class="oi oi-chevron-bottom float-right"></i>
        </div>
    </PickerTemplate>
</DateRangePicker>
````
Set id="@context.ParentId" for outside click handling to root element.

## Properties

| Name | Type | DefaultValue |  Description |
|------|------|--------------|--------------|
|StartDate|DateTimeOffset|null|The beginning date of the initially selected date range. If you provide a string, it must match the date format string set in your locale setting.|
|EndDate|DateTimeOffset|null|The end date of the initially selected date range.|
|MinDate|DateTimeOffset|null|The earliest date a user may select.|
|MaxDate|DateTimeOffset|null|The latest date a user may select.|
|MaxSpan|TimeSpan|null|The maximum span between the selected start and end dates. Check off maxSpan in the configuration generator for an example of how to use this. You can provide any object the moment library would let you add to a date.|
|ShowDropdowns|bool|true|Show year and month select boxes above calendars to jump to a specific month and year.|
|ShowWeekNumbers|bool|false|Show localized week numbers at the start of each week on the calendars.|
|ShowISOWeekNumbers|bool|false|Show ISO week numbers at the start of each week on the calendars.|
|Ranges|List<RangeItem>|null|Set predefined date ranges the user can select from. Each key is the label for the range, and its value an array with two dates representing the bounds of the range. Click ranges in the configuration generator for examples.|
|ShowCustomRangeLabel|bool|true|Displays "Custom Range" at the end of the list of predefined ranges, when the ranges option is used. This option will be highlighted whenever the current date range selection does not match one of the predefined ranges. Clicking it will display the calendars to select a new range.|
|AlwaysShowCalendars|bool|false|Normally, if you use the ranges option to specify pre-defined date ranges, calendars for choosing a custom date range are not shown until the user clicks "Custom Range". When this option is set to true, the calendars for choosing a custom date range are always shown instead.|
|Opens|SideType enum: Left/Right/Center|Right|Whether the picker appears aligned to the left, to the right, or centered under the HTML element it's attached to.|
|Drops|DrposType enum: Down/Up|Down|Whether the picker appears below (default) or above the HTML element it's attached to.|
|ButtonClasses|string|btn btn-sm|CSS class names that will be added to both the apply and cancel buttons.|
|ApplyButtonClasses|string|btn-primary|CSS class names that will be added only to the apply button.|
|CancelButtonClasses|string|btn-default|CSS class names that will be added only to the cancel button.|
|Culture|CultureInfo|CultureInfo.CurrentCulture|Allows you to provide localized strings for buttons and labels, customize the date format, and change the first day of week for the calendars.|
|SingleDatePicker|bool|false|Show only a single calendar to choose one date, instead of a range picker with two calendars. The start and end dates provided to your callback will be the same single date chosen.|
|AutoApply|bool|false|Hide the apply and cancel buttons, and automatically apply a new date range as soon as two dates are clicked.|
|LinkedCalendars|bool|false|When enabled, the two calendars displayed will always be for two sequential months (i.e. January and February), and both will be advanced when clicking the left or right arrows above the calendars. When disabled, the two calendars can be individually advanced and display any month/year.|
|DaysEnabledFunction|Func<DateTimeOffset,bool>|_ => true|A function that is passed each date in the two calendars before they are displayed, and may return true or false to indicate whether that date should be available for selection or not.|
|CustomDateFunction|Func<DateTimeOffset,bool>|_ => true|A function that is passed each date in the two calendars before they are displayed, and may return a string or array of CSS class names to apply to that date's calendar cell.|
|CustomDateClass|string|string.Empty|String of CSS class name to apply to that custom date's calendar cell|
|ApplyLabel|string|"Apply"|Apply button text|
|CancelLabel|string|"Cancel"|Cancel button text|
|CustomRangeLabel|string|"Custom range"|Custom range label at the end of the list of predefined ranges|


## Events

| Name | Type | Description |
|------|------|-------------|
|OnRangeSelect|DateRange|Triggered when the apply button is clicked, or when a predefined range is clicked|
|OnOpened|void|An event that is invoked when the DatePicker is opened|
|OnClosed|void|An event that is invoked when the DatePicker is closed|


## Methods

| Name |Description |
|------|------------|
|Open|Show picker popup|
|Close|Close picker popup|
|Toggle|Toggle picker popup state|


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

 
 
RangeItem:
````C#
public class RangeItem
{
    public string Name { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
}
````

## License

The MIT License (MIT)

Copyright (c) 2019 Sergey Zaikin

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