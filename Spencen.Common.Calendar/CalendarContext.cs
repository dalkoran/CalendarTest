namespace Spencen.Common.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class CalendarContext : IDisposable
    {
        private IDictionary<string, ICalendar> calendars;

        public CalendarContext()
        {
            this.calendars = new Dictionary<string, ICalendar>();
        }

        public CalendarContext(params ICalendar[] calendars)
        {
            this.calendars = calendars.ToDictionary(c => c.Key);
        }

        public DateTime GetNextBusinessDay(string calendarKey, DateTime asOfDate)
        {
            return this.GetNextBusinessDay(this.FetchCalendar(calendarKey), asOfDate);
        }

        public DateTime GetNextBusinessDay(ICalendar calendar, DateTime asOfDate)
        {
            return calendar.GetNextBusinessDay(asOfDate);
        }

        public DateTime GetNextBusinessDay(string[] calendarKeys, DateTime asOfDate)
        {
            var calendar = CalendarFactory.MergeCalendars("__Composite", this.calendars.Values.Where(c => calendarKeys.Contains(c.Key)));
            return calendar.GetNextBusinessDay(asOfDate);
        }

        public DateTime AddBusinessDay(string calendarKey, DateTime asOfDate, int numberOfDays)
        {
            return this.AddBusinessDay(this.FetchCalendar(calendarKey), asOfDate, numberOfDays);
        }

        public DateTime AddBusinessDay(ICalendar calendar, DateTime asOfDate, int numberOfDays)
        {
            return calendar.AddBusinessDay(asOfDate, numberOfDays);
        }

        public DateTime AddBusinessDay(string[] calendarKeys, DateTime asOfDate, int numberOfDays)
        {
            var calendar = CalendarFactory.MergeCalendars("__Composite", this.calendars.Values.Where(c => calendarKeys.Contains(c.Key)));
            return this.AddBusinessDay(calendar, asOfDate, numberOfDays);
        }

        public IEnumerable<DateTime> GetBusinessDays(string calendarKey, DateRange range, Predicate<DateTime> predicate = null)
        {
            return this.FetchCalendar(calendarKey).GetBusinessDays(range, predicate);
        }

        public IEnumerable<DateTime> GetFirstBusinessDaysOfWeek(string calendarKey, DateRange range, int weekDayOffset = 1)
        {
            return this.GetFirstBusinessDaysOfWeek(this.FetchCalendar(calendarKey), range, weekDayOffset);
        }

        public IEnumerable<DateTime> GetLastBusinessDaysOfWeek(string calendarKey, DateRange range, int weekDayOffset = 1)
        {
            return this.GetLastBusinessDaysOfWeek(this.FetchCalendar(calendarKey), range, weekDayOffset);
        }

        public IEnumerable<DateTime> GetFirstBusinessDaysOfWeek(ICalendar calendar, DateRange range, int weekDayOffset = 1)
        {
            return calendar.GetFirstBusinessDaysOfWeek(range, weekDayOffset);
        }

        public IEnumerable<DateTime> GetLastBusinessDaysOfWeek(ICalendar calendar, DateRange range, int weekDayOffset = 1)
        {
            return calendar.GetLastBusinessDaysOfWeek(range, weekDayOffset);
        }

        private ICalendar FetchCalendar(string calendarKey)
        {
            try
            {
                return this.calendars[calendarKey];
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"The specified calendar key '{calendarKey}' does not exist within the context.", ex);
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                this.disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

        #endregion
    }
}
