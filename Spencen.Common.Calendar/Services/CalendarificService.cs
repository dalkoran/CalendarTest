﻿namespace Spencen.Common.Calendar.Services
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Spencen.Common.Calendar.Calendars;

    public class CalendarificService : ICalendarService
    {
        private const string ApiKey = "ec998632af1814035d6f807265e57ed6722e0155";
        private const string RequestUri = @"https://calendarific.com/api/v2/holidays?country={0}&year={1}&type=national&api_key={2}";

        private static readonly object cacheLock = new object();
        private static MemoryCache cache;

        static CalendarificService()
        {
            IOptions<MemoryCacheOptions> options = Options.Create<MemoryCacheOptions>(new MemoryCacheOptions());
            cache = new MemoryCache(options);
        }

        public MemoryCache Cache
        {
            get => cache;
        }

        public async Task<ICalendar> GetCalendarAsync(string key, DateRange dateRange)
        {
            if (TryFetchFromCache(key, dateRange.BeginDate.Value.Year, out ICalendar cachedCalendar))
            {
                return cachedCalendar;
            }

            var httpClient = new HttpClient();
            var request = string.Format(RequestUri, key, dateRange.BeginDate.Value.Year, ApiKey);
            var response = await httpClient.GetAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

                var document = JsonDocument.Parse(content);
                var holidayNode = document.RootElement.GetProperty("response").GetProperty("holidays");

                var holidays = JsonSerializer.Deserialize<CalendarificHoliday[]>(holidayNode.ToString(), options);

                var calendar = new SimpleCalendar(
                    key,
                    holidays.Select(h => new SimpleHoliday()
                    {
                        Description = h.Name,
                        Dates = new DateRange(h.Date.Value, h.Date.Value.AddDays(1).AddTicks(-1)),
                    }).ToArray(),
                    CalendarFactory.MondayToFridayWorkWeek);

                AddOrUpdateCache(key, dateRange.BeginDate.Value.Year, calendar);

                return calendar;
            }
            else
            {
                return null;
            }
        }

        private static bool TryFetchFromCache(string key, int year, out ICalendar calendar)
        {
            lock(cacheLock)
            {
                return (cache.TryGetValue<ICalendar>($"{key}:{year}", out calendar));
            }
        }

        private static void AddOrUpdateCache(string key, int year, ICalendar calendar)
        {
            lock(cacheLock)
            {
                cache.Set($"{key}:{year}", calendar, new TimeSpan(2, 0, 0)); // cache for 2 hours
            }
        }

        private class CalendarificHoliday
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public CalendarificDate Date { get; set; }
        }

        private class CalendarificDate
        {
            public CalendarificDateTime Datetime { get; set; }
            public DateTime Value => new DateTime(this.Datetime.Year, this.Datetime.Month, this.Datetime.Day);
        }

        private class CalendarificDateTime
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public int Day { get; set; }
        }
    }
}
