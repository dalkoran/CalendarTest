namespace Spencen.Common.Calendar.Calendars
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerDisplay("{Key}")]
    public class SimpleCalendar : ICalendar
    {
        public Func<DayOfWeek, bool> isWorkingDayFunc;

        public SimpleCalendar(string key, IEnumerable<IHoliday> holidays, Func<DayOfWeek, bool> isWorkingDayFunc)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (holidays== null) throw new ArgumentNullException(nameof(holidays));
            if (isWorkingDayFunc == null) throw new ArgumentNullException(nameof(isWorkingDayFunc));

            this.Key = key;
            this.Holidays = holidays;
            this.isWorkingDayFunc = isWorkingDayFunc;
        }

        public string Key { get; }

        public IEnumerable<IHoliday> Holidays { get; }

        public virtual bool IsWorkingDay(DayOfWeek dayOfWeek)
        {
            return this.isWorkingDayFunc(dayOfWeek);
        }
    }
}
