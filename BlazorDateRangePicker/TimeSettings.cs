using System.Collections.Generic;

namespace BlazorDateRangePicker
{
    public class TimeSettings
    {
        public IEnumerable<int> Hours { get; set; }
        public IEnumerable<int> Minutes { get; set; }
        public IEnumerable<int> Seconds { get; set; }
    }
}
