namespace Spencen.Common.Calendar.Calendars
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{Description,nq} {Dates}")]
    public class SimpleHoliday : IHoliday
    {
        public SimpleHoliday() { }

        public SimpleHoliday(DateTime date, string description)
            : base()
        {
            this.Dates = new DateRange(date.Date, date.Date.AddDays(1).AddTicks(-1));
            this.Description = description;
        }

        public DateRange Dates { get; set; }

        public string Description { get; set; }
    }
}
