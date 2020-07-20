namespace Spencen.Common.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Net.NetworkInformation;
    using System.Text;

    public static class DateExtensions
    {
        public static DateTime NextDayOfWeek(this DateTime asOf, DayOfWeek dayOfWeek, int next = 1)
        {
            var offset = ((7 + (int)dayOfWeek) - (int)asOf.DayOfWeek) % 7;
            return asOf.AddDays(offset + (next - 1) * 7);
        }

        public static DateTime PreviousDayOfWeek(this DateTime asOf, DayOfWeek dayOfWeek, int next = 1)
        {
            var offset = ((7 + (int)dayOfWeek) - (int)asOf.DayOfWeek) % 7;
            return asOf.AddDays(offset - (next * 7));
        }

        public static DateTime NextMonth(this DateTime asOf, int next = 1, Func<DateTime, bool> filter = null)
        {
            filter = filter ?? new Func<DateTime, bool>(_ => true);
            int validCount = 0;
            while (validCount < next)
            {
                asOf = asOf.AddMonths(1);
                if (filter(asOf))
                {
                    validCount++;
                }
            }

            return asOf;
        }

        public static DateTime FirstDayOfMonth(this DateTime asOf)
        {
            return new DateTime(asOf.Year, asOf.Month, 1, asOf.Hour, asOf.Minute, asOf.Second, asOf.Millisecond);
        }
    }
}
