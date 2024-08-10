using SMS.Interfaces;

namespace SMS.Generic
{
    public class DateTimeRange : IDateTimeRange
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
