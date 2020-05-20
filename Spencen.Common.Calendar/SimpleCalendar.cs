namespace Spencen.Common.Calendar
{
    using System.Collections.Generic;

    public class SimpleCalendar : ICalendar
    {
        public string Key { get; set; }

        public IEnumerable<IHoliday> Holidays { get; set; }
    }
}
