namespace Spencen.Common.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CalendarExtension
    {
        public static bool IsHoliday(this ICalendar calendar, DateTime date)
        {
            return calendar.Holidays.Any(h => h.Dates.Contains(date));
        }

        public static bool IsNormalWorkingDay(this ICalendar calendar, DateTime date)
        {
            return calendar.IsWorkingDay(date.DayOfWeek);
        }

        public static IEnumerable<DateTime> GetBusinessDays(this ICalendar calendar, DateRange range, Predicate<DateTime> predicate = null)
        {
            if (range.IsInfinite)
            {
                throw new ArgumentOutOfRangeException(nameof(range), "The date range used to iterate business days cannot be infinite, it must have a set begin and end date.");
            }

            var asOfDate = range.BeginDate.Value.Date;
            while (asOfDate < range.EndDate.Value)
            {
                if (calendar.IsNormalWorkingDay(asOfDate) && !calendar.IsHoliday(asOfDate))
                {
                    if (predicate == null || predicate(asOfDate))
                    {
                        yield return asOfDate;
                    }
                }

                asOfDate = asOfDate.AddDays(1);
            }

            yield break;
        }

        public static DateTime GetCurrentOrNextBusinessDay(this ICalendar calendar, DateTime asOfDate)
        {
            asOfDate = asOfDate.Date; // Ignore any time component
            do
            {
                if (calendar.IsNormalWorkingDay(asOfDate) && !calendar.IsHoliday(asOfDate))
                {
                    break;
                }

                // Skip holidays and weekends
                asOfDate = asOfDate.AddDays(1);
            } while (asOfDate < DateTime.MaxValue);

            return asOfDate;
        }

        public static DateTime GetCurrentOrPriorBusinessDay(this ICalendar calendar, DateTime asOfDate)
        {
            asOfDate = asOfDate.Date; // Ignore any time component
            do
            {
                if (calendar.IsNormalWorkingDay(asOfDate) && !calendar.IsHoliday(asOfDate))
                {
                    break;
                }

                // Skip holidays and weekends
                asOfDate = asOfDate.AddDays(-1);
            } while (asOfDate < DateTime.MinValue);

            return asOfDate;
        }

        public static DateTime AddBusinessDays(this ICalendar calendar, DateTime asOfDate, int numberOfDays)
        {
            // If we need the next business day forwards, then start from the last business day (this handles 0)
            var offset = 1;
            if (numberOfDays > 0)
            {
                asOfDate = GetCurrentOrPriorBusinessDay(calendar, asOfDate); // Start from prior business day (today inclusive)
            }
            else
            {
                asOfDate = GetCurrentOrNextBusinessDay(calendar, asOfDate); // Start from current or next business day
                offset = -1;
                numberOfDays = Math.Abs(numberOfDays);
            }

            var businessDayCount = 0;
            while (asOfDate < DateTime.MaxValue && businessDayCount < numberOfDays)
            {
                asOfDate = asOfDate.AddDays(offset);

                if (calendar.IsNormalWorkingDay(asOfDate) && !calendar.IsHoliday(asOfDate))
                {
                    businessDayCount++;
                }
            }

            return asOfDate;
        }

        public static IEnumerable<DateTime> GetFirstBusinessDaysOfWeek(this ICalendar calendar, DateRange range, int weekDayOffset = 1)
        {
            return calendar.GetBusinessDaysOfWeek(range, weekDayOffset, 1);
        }

        public static IEnumerable<DateTime> GetLastBusinessDaysOfWeek(this ICalendar calendar, DateRange range, int weekDayOffset = 1)
        {
            return calendar.GetBusinessDaysOfWeek(range, weekDayOffset, -1).Reverse();
        }

        public static IEnumerable<DateTime> GetHolidays(this ICalendar calendar, DateRange range)
        {
            foreach (var holiday in calendar.Holidays)
            {
                foreach (var day in holiday.Dates.GetDays())
                {
                    // Exclude weekends from holidays
                    if (calendar.IsNormalWorkingDay(day))
                    {
                        yield return day;
                    }
                }
            }

            yield break;
        }

        private static IEnumerable<DateTime> GetBusinessDaysOfWeek(this ICalendar calendar, DateRange range, int weekDayOffset, int stepDirection)
        {
            if (range.IsInfinite)
            {
                throw new ArgumentOutOfRangeException(nameof(range), "The date range used to iterate business days cannot be infinite, it must have a set begin and end date.");
            }

            if (weekDayOffset < 1 || weekDayOffset > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(weekDayOffset), "The offset must be within the range 1 to 7 (first to seventh day of week).");
            }

            // Expand the range to include full weeks
            var fullWeekRange = new DateRange(
                range.BeginDate.Value.AddDays(-(int)range.BeginDate.Value.DayOfWeek),
                range.EndDate.Value.AddDays(6 - (int)range.EndDate.Value.DayOfWeek));

            DateTime asOfDate;
            switch (stepDirection)
            {
                case 1:
                    asOfDate = fullWeekRange.BeginDate.Value.Date;
                    break;
                case -1:
                    asOfDate = fullWeekRange.EndDate.Value.Date;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stepDirection), "Must be either 1 or -1.");
            }

            ////var weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(asOfDate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
            var workingDay = 0;
            while (fullWeekRange.Contains(asOfDate))
            {
                if (calendar.IsNormalWorkingDay(asOfDate) && !calendar.IsHoliday(asOfDate))
                {
                    workingDay++;
                    if (workingDay == weekDayOffset)
                    {
                        if (range.Contains(asOfDate))
                        {
                            yield return asOfDate;
                        }
                    }
                }

                asOfDate = asOfDate.AddDays(stepDirection);
                if (asOfDate.DayOfWeek == (stepDirection > 0 ? DayOfWeek.Sunday : DayOfWeek.Saturday))
                {
                    ////weekNumber++; // increment week
                    workingDay = 0; // reset nth day of working week
                }
            }

            yield break;
        }
    }
}
