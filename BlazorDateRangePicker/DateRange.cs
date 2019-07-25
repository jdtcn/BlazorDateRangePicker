/**
* author: Sergey Zaikin zaikinsr@yandex.ru
* copyright: Copyright (c) 2019 Sergey Zaikin. All rights reserved.
* license: Licensed under the MIT license. See http://www.opensource.org/licenses/mit-license.php
*/

using System;

namespace BlazorDateRangePicker
{

    public class DateRange
    {
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
    }
}
