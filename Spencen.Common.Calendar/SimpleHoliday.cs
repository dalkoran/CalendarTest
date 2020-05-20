namespace Spencen.Common.Calendar
{
    public class SimpleHoliday : IHoliday
    {
        public DateRange Dates { get; set; }

        public string Description { get; set; }
    }
}
