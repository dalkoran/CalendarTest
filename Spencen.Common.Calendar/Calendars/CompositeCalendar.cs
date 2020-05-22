namespace Spencen.Common.Calendar.Calendars
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CompositeCalendar : ICalendar
    {
        private IEnumerable<ICalendar> childCalendars;

        public CompositeCalendar(string key, IEnumerable<ICalendar> calendars)
        {
            this.Key = key;
            this.childCalendars = calendars;
        }

        public string Key { get; }

        public IEnumerable<IHoliday> Holidays
        {
            get
            {
                // Simple version - don't try and merge calendars
                foreach (var calendar in this.childCalendars)
                {
                    foreach (var holiday in calendar.Holidays)
                    {
                        yield return holiday;
                    }
                }
            }
        }

        public bool IsWorkingDay(DayOfWeek dayOfWeek)
        {
            return this.childCalendars.All(c => c.IsWorkingDay(dayOfWeek));
        }
    }
}
