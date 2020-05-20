namespace Spencen.Common.Calendar
{
    public interface IHoliday
    {
        DateRange Dates { get; }
        string Description { get; }
    }
}
