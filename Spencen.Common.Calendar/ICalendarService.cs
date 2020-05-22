namespace Spencen.Common.Calendar
{
    using System.Threading.Tasks;

    public interface ICalendarService
    {
        Task<ICalendar> GetCalendarAsync(string key, DateRange dateRange);
    }
}
