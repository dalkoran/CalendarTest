namespace Spencen.Common.Calendar
{
    public interface IHoliday
    {
        /// <summary>
        /// Gets the range of dates (must be specified as full days) that are considered a holiday (non-working day).
        /// </summary>
        DateRange Dates { get; }
        
        /// <summary>
        /// Gets an optional description or the holiday, e.g. "Memorial Day".
        /// </summary>
        string Description { get; }
    }
}
