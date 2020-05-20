namespace Spencen.Common.Calendar
{
    using System.Collections.Generic;
    using System.Linq;

    public static class CalendarFactory
    {
        public static ICalendar CreateFromDateRanges(string key, IEnumerable<DateRange> dateRanges)
        {
            return new SimpleCalendar
            {
                Key = key,
                Holidays = dateRanges.Select(dr => new SimpleHoliday() { Dates = dr, Description = null }).ToArray(),
            };
        }

        public static ICalendar MergeCalendars(string key, IEnumerable<ICalendar> calendars)
        {
            return new CompositeCalendar(key, calendars);
        }
    }
}
