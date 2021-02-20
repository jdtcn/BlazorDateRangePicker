/**
* @author: Sergey Zaikin zaikinsr@yandex.ru
* @copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* @license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace BlazorDateRangePicker
{
    public partial class TimePicker
    {
        [CascadingParameter] public DateRangePicker Picker { get; set; }
        [Parameter] public SideType Side { get; set; }
        [Parameter] public TimeSpan Time { get; set; }
        [Parameter] public EventCallback<TimeSpan?> TimeChanged { get; set; }

        private IEnumerable<int> HoursRange =>
            Enumerable.Range(
                Picker.TimePicker24Hour == true ? 0 : 1,
                Picker.TimePicker24Hour == true ? 24 : 12);

        private AmPmEnum AmPm
        {
            get => Time.Hours >= 12 ? AmPmEnum.PM : AmPmEnum.AM;
            set
            {
                Set(h: value switch
                {
                    AmPmEnum.AM when Time.Hours >= 12 => Time.Hours - 12,
                    AmPmEnum.PM when Time.Hours < 12 => Time.Hours + 12,
                    _ => Time.Hours
                });
            }
        }

        private int Hour
        {
            get
            {
                if (Picker.TimePicker24Hour == true) return Time.Hours;
                return int.Parse(DateTime.Today.Add(Time).ToString("hh"));
            }
            set
            {
                if (Picker.TimePicker24Hour == true) Set(h: value);
                Set(h: AmPm switch
                {
                    AmPmEnum.AM when value >= 12 => value - 12,
                    AmPmEnum.PM when value < 12 => value + 12,
                    _ => value
                });
            }
        }

        private int Minute { get => Time.Minutes; set => Set(m: value); }
        private int Second { get => Time.Seconds; set => Set(s: value); }

        private void Set(int? h = null, int? m = null, int? s = null)
        {
            Time = TimeSpan.FromHours(h ?? Time.Hours)
                 + TimeSpan.FromMinutes(m ?? Minute)
                 + TimeSpan.FromSeconds(s ?? Second);

            TimeChanged.InvokeAsync(Time);
        }
    }

    public enum AmPmEnum
    {
        AM, PM
    }
}