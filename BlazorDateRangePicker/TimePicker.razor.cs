/**
* @author: Sergey Zaikin zaikinsr@yandex.ru
* @copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* @license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace BlazorDateRangePicker
{
    public partial class TimePicker
    {
        [CascadingParameter] public DateRangePicker Picker { get; set; }
        [Parameter] public SideType Side { get; set; }
        [Parameter] public TimeSpan Time { get; set; }
        [Parameter] public EventCallback<TimeSpan> TimeChanged { get; set; }
        [Parameter] public DateTimeOffset? Day { get; set; }
        [Parameter] public bool? TimePicker24Hour { get; set; }

        private readonly IEnumerable<int> HoursRange24 = Enumerable.Range(0, 24);
        private readonly IEnumerable<int> HoursRange12 = Enumerable.Range(1, 12);
        private IEnumerable<int> CustomHoursRange;

        protected override void OnInitialized()
        {
            MinutesRange = Enumerable.Range(0, 60);
            SecondsRange = Enumerable.Range(0, 60);
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Picker.TimeEnabledFunction != null)
            {
                var timeEnabled = await Picker.TimeEnabledFunction(Day);
                CustomHoursRange = timeEnabled.Hours;
                MinutesRange = timeEnabled.Minutes;
                SecondsRange = timeEnabled.Seconds;
            }
            else
            {
                CustomHoursRange = null;
                MinutesRange = Enumerable.Range(0, 60);
                SecondsRange = Enumerable.Range(0, 60);
            }
        }

        private IEnumerable<int> HoursRange => CustomHoursRange ?? (TimePicker24Hour != false ? HoursRange24 : HoursRange12);
        private IEnumerable<int> MinutesRange { get; set; }
        private IEnumerable<int> SecondsRange { get; set; }

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
                Set(h: AmPm switch
                {
                    AmPmEnum.AM when TimePicker24Hour == false && value >= 12 => value - 12,
                    AmPmEnum.PM when TimePicker24Hour == false && value < 12 => value + 12,
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