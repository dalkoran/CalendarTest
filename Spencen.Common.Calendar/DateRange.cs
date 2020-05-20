namespace Spencen.Common.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;

    public struct DateRange
    {
        public DateRange(DateTime? beginDate, DateTime? endDate)
        {
            this.BeginDate = beginDate;
            this.EndDate = endDate;
        }

        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsInfinite { get => !this.BeginDate.HasValue || !this.EndDate.HasValue; }

        public bool Contains(DateTime date)
        {
            return date >= (this.BeginDate ?? DateTime.MinValue) && date <= (this.EndDate ?? DateTime.MaxValue);
        }

        public IEnumerable<DateTime> GetDays()
        {
            if (this.IsInfinite)
            {
                yield break;
            }

            var asOfDate = this.BeginDate.Value;
            while (this.Contains(asOfDate))
            {
                yield return asOfDate;
                asOfDate = asOfDate.AddDays(1);
            }

            yield break;
        }
    }
}
