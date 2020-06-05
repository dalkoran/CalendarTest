namespace Spencen.Common.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Spencen.Common.Calendar.Calendars;

    public static class CalendarFactory
    {
        /// <summary>
        /// Creates a simple ICalendar implementation from an enumeration of date ranges, where each item
        /// in the enumeration is considered to be a holiday.
        /// </summary>
        /// <param name="key">A key to uniquely identify the calendar.</param>
        /// <param name="dateRanges">An enumeration of date ranges that are considered to be holidays.</param>
        /// <param name="isWorkingDayFunc">A function that determines whether or not a day of the week is a working day (excluding holidays). 
        /// This will default to a standard five day working week from Monday through Friday inclusive.</param>
        /// <returns>An instance that implements the ICalendar interface.</returns>
        public static ICalendar CreateFromDateRanges(string key, IEnumerable<DateRange> dateRanges, Func<DayOfWeek, bool> isWorkingDayFunc = null)
        {
            return new SimpleCalendar(
                key,
                dateRanges.Select(dr => new SimpleHoliday() { Dates = dr, Description = null }).ToArray(),
                isWorkingDayFunc ?? MondayToFridayWorkWeek);
        }

        public static ICalendar MergeCalendars(string key, IEnumerable<ICalendar> calendars)
        {
            return new CompositeCalendar(key, calendars);
        }

        public static Func<DayOfWeek, bool> MondayToFridayWorkWeek
        {
            get => (DayOfWeek dayOfWeek) =>
                dayOfWeek != DayOfWeek.Saturday &&
                dayOfWeek != DayOfWeek.Sunday;
        }

        public static Func<DayOfWeek, bool> SevenDayWorkWeek
        {
            get => _ => true;
        }

        public static ICalendar BusinessCalendar
        {
            get => new SimpleCalendar(CalendarContext.CalendarNames.BusinessDay, Enumerable.Empty<IHoliday>(), MondayToFridayWorkWeek);
        }

        public static ICalendar Calendar
        {
            get => new SimpleCalendar(CalendarContext.CalendarNames.CalendarDay, Enumerable.Empty<IHoliday>(), SevenDayWorkWeek);
        }
    }
}
