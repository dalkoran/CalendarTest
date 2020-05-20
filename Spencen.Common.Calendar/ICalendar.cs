namespace Spencen.Common.Calendar
{
    using System.Collections.Generic;

    public interface ICalendar
    {
        string Key { get; }
        IEnumerable<IHoliday> Holidays { get; }
    }
}
