namespace Spencen.Common.Calendar
{
    using System;
    using System.Collections.Generic;

    public interface ICalendar
    {
        /// <summary>
        /// Gets a key to unqiuely identify a calendar within a collection.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the holidays specific to this calendar. Holidays do not need to include days of the week
        /// that are non-working days, e.g. IsWorkingDay == false.
        /// </summary>
        IEnumerable<IHoliday> Holidays { get; }

        /// <summary>
        /// Determines whether the specified <paramref name="dayOfWeek"/> is considered to be a working
        /// day for this calendar.
        /// </summary>
        /// <param name="dayOfWeek">The day of the week being checked.</param>
        /// <returns>True if the day of the week is normally considered a working day (excluding holidays), false otherwise.</returns>
        bool IsWorkingDay(DayOfWeek dayOfWeek);
    }
}
