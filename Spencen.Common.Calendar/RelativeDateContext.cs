namespace Spencen.Common.Calendar
{
    using System;

    internal class RelativeDateContext
    {
        public RelativeDateContext(ICalendar calendar, TimeZoneInfo timeZone, int number, DateTime asOf)
        {
            this.Calendar = calendar;
            this.TimeZone = timeZone;
            this.Number = number;
            this.AsOf = asOf;
        }

        public ICalendar Calendar { get; }
        public TimeZoneInfo TimeZone { get; }
        public int Number { get; }
        public DateTime AsOf { get; }
    }
}
