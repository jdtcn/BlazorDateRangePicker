/**
* @author: Sergey Zaikin zaikinsr@yandex.ru
* @copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* @license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/

namespace BlazorDateRangePicker
{
    public partial class DateRangePickerConfig : IConfigurableOptions
    {
        public DateRangePickerConfig()
        {
            // Set default values
            ButtonClasses = "btn btn-sm";
            ApplyButtonClasses = "btn-primary";
            CancelButtonClasses = "btn-default";
            ApplyLabel = "Apply";
            CancelLabel = "Cancel";
            CustomRangeLabel = "Custom Range";
            WeekAbbreviation = string.Empty;

            ShowDropdowns = true;
            ShowCustomRangeLabel = true;
            Inline = false;
            CloseOnOutsideClick = true;
            AutoAdjustCalendars = true;
            ResetOnClear = true;
            TimePicker = false;
            TimePickerSeconds = false;
            Prerender = true;
            TimePickerIncrement = 1;

            Culture = System.Globalization.CultureInfo.CurrentCulture;
            Opens = SideType.Right;
            Drops = DropsType.Down;

            CustomDateFunction = _ => false;
        }
    }
}
