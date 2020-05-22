namespace CalendarTest
{
    using System;
    using System.Threading.Tasks;
    using Spencen.Common.Calendar;
    using Spencen.Common.Calendar.Services;

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var dateRange = new DateRange(new DateTime(DateTime.Today.Year, 1, 1), new DateTime(DateTime.Today.Year, 12, 31));
            var service = new CalendarificService();
            var calendar = await service.GetCalendarAsync("us", dateRange);
            var holidays = calendar.GetHolidays(dateRange);
        }
    }
}
