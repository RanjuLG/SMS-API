using SMS.Interfaces;

namespace SMS.Generic
{
    public class DateTimeRange : IDateTimeRange
    {
        public DateTime From { get; set; } = DateTime.MinValue;
        public DateTime To { get; set; } = DateTime.MinValue;
    }
}
