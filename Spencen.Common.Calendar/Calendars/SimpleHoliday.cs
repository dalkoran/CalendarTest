using System.Diagnostics;

namespace Spencen.Common.Calendar.Calendars
{
    [DebuggerDisplay("{Description,nq} {Dates}")]
    public class SimpleHoliday : IHoliday
    {
        public DateRange Dates { get; set; }

        public string Description { get; set; }
    }
}
