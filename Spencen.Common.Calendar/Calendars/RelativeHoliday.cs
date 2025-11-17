namespace Spencen.Common.Calendar.Calendars
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class RelativeHoliday : IHoliday
    {
        public RelativeHoliday(RelativeDate relativeDate, DateTime baseDate, string description)
        {
            this.RelativeDate = relativeDate;
            this.BaseDate = baseDate;
            this.Description = description;
        }

        public DateRange Dates
        {
            get
            {
                var resolvedDate = this.RelativeDate.Apply(this.BaseDate);
                return new DateRange(resolvedDate, resolvedDate);
            }
        }

        public RelativeDate RelativeDate { get; private set; }

        public DateTime BaseDate { get; private set; }

        public string Description { get; private set; }

        public static class UnitedStates
        {
            public static RelativeDate NewYearsDay = new RelativeDate("@1M@1d?sat{+2d}?sun{+d}");
            public static RelativeDate MartinLutherKingJrDay = new RelativeDate("@1M@3mon");
            public static RelativeDate PresidentsDay = new RelativeDate("@2M@3mon");
            public static RelativeDate MemorialDay = new RelativeDate("@5M@25d?sat{-d}?sun{+d}");
            public static RelativeDate Juneteenth = new RelativeDate("@6M@19d");
            public static RelativeDate FourthOfJuly = new RelativeDate("@7M@4d?sat{-d}?sun{+d}");
            public static RelativeDate LaborDay = new RelativeDate("@9M@1mon");
            public static RelativeDate IndigineousPeoplesDay = new RelativeDate("@10M@2mon");
            public static RelativeDate Thanksgiving = new RelativeDate("@11M@4thu");
            public static RelativeDate ChristmasDay = new RelativeDate("@12M@25d?sat{-d}?sun{+d}");
        }
    }
}
