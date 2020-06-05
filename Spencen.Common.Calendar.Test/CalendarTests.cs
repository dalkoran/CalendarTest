namespace Spencen.Common.Calendar.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Spencen.Common.Calendar.Services;

    [TestClass]
    public class CalendarTests
    {
        private ICalendar calendar;
        private ICalendar personalCalendar;

        [TestInitialize]
        public void Initialize()
        {
            this.calendar = CalendarFactory.CreateFromDateRanges(
                "CAL",
                new[]
                {
                    new DateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 1, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 1, 20), new DateTime(2020, 1, 20, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 2, 17), new DateTime(2020, 2, 17, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 4, 10), new DateTime(2020, 4, 10, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 5, 25), new DateTime(2020, 5, 25, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 7, 3), new DateTime(2020, 7, 3, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 9, 7), new DateTime(2020, 9, 7, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 11, 26), new DateTime(2020, 11, 26, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 12, 25), new DateTime(2020, 12, 25, 23, 59, 59)),
                });

            this.personalCalendar = CalendarFactory.CreateFromDateRanges(
                "Personal",
                new[]
                {
                    new DateRange(new DateTime(2020, 4, 6), new DateTime(2020, 4, 9, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 8, 10), new DateTime(2020, 8, 21, 23, 59, 59)),
                });
        }

        [TestMethod]
        public void Returns_Today_For_Weekday()
        {
            var asOfDate = new DateTime(2020, 5, 13); // Wednesday
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                var result = calendarContext.GetNextBusinessDay(this.calendar.Key, asOfDate);

                Assert.AreEqual(asOfDate, result);
            }
        }

        [TestMethod]
        public void Returns_Monday_For_Weekend()
        {
            var asOfDate = new DateTime(2020, 5, 16); // Saturday
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                var result = calendarContext.GetNextBusinessDay(this.calendar.Key, asOfDate);

                Assert.AreEqual(new DateTime(2020, 5, 18), result);
            }
        }

        [TestMethod]
        public void Returns_Tuesday_For_MemorialDay()
        {
            var asOfDate = new DateTime(2020, 5, 25); // Memorial Day
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                var result = calendarContext.GetNextBusinessDay(this.calendar.Key, asOfDate);

                Assert.AreEqual(new DateTime(2020, 5, 26), result);
            }
        }

        [TestMethod]
        public void Returns_Easter_Monday_For_April_Break()
        {
            var asOfDate = new DateTime(2020, 4, 3); // Friday before week of Easter Friday (and personal vacation)
            using (var calendarContext = new CalendarContext(this.calendar, this.personalCalendar))
            {
                var result = calendarContext.AddBusinessDay(new[] { this.calendar.Key, this.personalCalendar.Key }, asOfDate, 1);

                Assert.AreEqual(new DateTime(2020, 4, 13), result, "Easter Monday after 4 day vacation and Good Friday.");
            }
        }

        [TestMethod]
        public void Returns_Easter_Monday_For_April_Break_Starting_on_Weekend_Next_Business_Day()
        {
            var asOfDate = new DateTime(2020, 4, 4); // Saturday before week of Easter Friday (and personal vacation)
            using (var calendarContext = new CalendarContext(this.calendar, this.personalCalendar))
            {
                var result = calendarContext.GetNextBusinessDay(new[] { this.calendar.Key, this.personalCalendar.Key }, asOfDate);

                Assert.AreEqual(new DateTime(2020, 4, 13), result, "Easter Monday after 4 day vacation and Good Friday.");
            }
        }

        [TestMethod]
        public void Returns_Easter_Monday_For_April_Break_Starting_on_Weekend_Add_Business_Day()
        {
            var asOfDate = new DateTime(2020, 4, 4); // Saturday before week of Easter Friday (and personal vacation)
            using (var calendarContext = new CalendarContext(this.calendar, this.personalCalendar))
            {
                var result = calendarContext.AddBusinessDay(new[] { this.calendar.Key, this.personalCalendar.Key }, asOfDate, 1);

                Assert.AreEqual(new DateTime(2020, 4, 13), result, "Easter Monday after 4 day vacation and Good Friday.");
            }
        }

        [TestMethod]
        public void Add_One_Business_Day_To_Monday()
        {
            var asOfDate = new DateTime(2020, 5, 18); // Monday
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                var result = calendarContext.AddBusinessDay(this.calendar, asOfDate, 1);

                Assert.AreEqual(new DateTime(2020, 5, 19), result);
            }
        }

        [TestMethod]
        public void Add_One_Business_Day_To_Friday()
        {
            var asOfDate = new DateTime(2020, 5, 15); // Friday
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                var result = calendarContext.AddBusinessDay(this.calendar, asOfDate, 1);

                Assert.AreEqual(new DateTime(2020, 5, 18), result);
            }
        }

        [TestMethod]
        public void Add_One_Business_Day_To_Friday_Prior_To_Memorial_Day()
        {
            var asOfDate = new DateTime(2020, 5, 22); // Friday
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                var result = calendarContext.AddBusinessDay(this.calendar, asOfDate, 1);

                Assert.AreEqual(new DateTime(2020, 5, 26), result);
            }
        }

        [TestMethod]
        public void Get_Monday_of_Week_In_May()
        {
            var dateRange = new DateRange(new DateTime(2020, 5, 1), new DateTime(2020, 5, 31));
            Predicate<DateTime> predicate = d => d.DayOfWeek == DayOfWeek.Monday;
            IList<DateTime> result;
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                result = calendarContext.GetBusinessDays(this.calendar.Key, dateRange, predicate).ToList();
            }

            Assert.AreEqual(3, result.Count, "Only three business Mondays in May");
            Assert.AreEqual(new DateTime(2020, 5, 4), result[0]);
            Assert.AreEqual(new DateTime(2020, 5, 11), result[1]);
            Assert.AreEqual(new DateTime(2020, 5, 18), result[2]);
        }

        [TestMethod]
        public void Get_First_Working_Day_of_Weeks_In_May()
        {
            var dateRange = new DateRange(new DateTime(2020, 5, 1), new DateTime(2020, 5, 31));
            IList<DateTime> result;
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                result = calendarContext.GetFirstBusinessDaysOfWeek(this.calendar.Key, dateRange, 1).ToList();
            }

            Assert.AreEqual(4, result.Count, "Only four weeks start in May");
            Assert.AreEqual(new DateTime(2020, 5, 4), result[0]);
            Assert.AreEqual(new DateTime(2020, 5, 11), result[1]);
            Assert.AreEqual(new DateTime(2020, 5, 18), result[2]);
            Assert.AreEqual(new DateTime(2020, 5, 26), result[3]);
        }

        [TestMethod]
        public void Get_Last_Working_Day_of_Weeks_In_May()
        {
            var dateRange = new DateRange(new DateTime(2020, 5, 1), new DateTime(2020, 5, 31));
            IList<DateTime> result;
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                result = calendarContext.GetLastBusinessDaysOfWeek(this.calendar.Key, dateRange, 1).ToList();
            }

            Assert.AreEqual(5, result.Count, "Only five weeks end in May");
            Assert.AreEqual(new DateTime(2020, 5, 1), result[0]);
            Assert.AreEqual(new DateTime(2020, 5, 8), result[1]);
            Assert.AreEqual(new DateTime(2020, 5, 15), result[2]);
            Assert.AreEqual(new DateTime(2020, 5, 22), result[3]);
            Assert.AreEqual(new DateTime(2020, 5, 29), result[4]);
        }

        [TestMethod]
        public void Get_First_Working_Day_of_Weeks_In_April()
        {
            var dateRange = new DateRange(new DateTime(2020, 4, 1), new DateTime(2020, 4, 30));
            IList<DateTime> result;
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                result = calendarContext.GetFirstBusinessDaysOfWeek(this.calendar.Key, dateRange, 1).ToList();
            }

            Assert.AreEqual(4, result.Count, "Only four weeks start in April");
            Assert.AreEqual(new DateTime(2020, 4, 6), result[0]);
            Assert.AreEqual(new DateTime(2020, 4, 13), result[1]);
            Assert.AreEqual(new DateTime(2020, 4, 20), result[2]);
            Assert.AreEqual(new DateTime(2020, 4, 27), result[3]);
        }

        [TestMethod]
        public void Get_Last_Working_Day_of_Weeks_In_April()
        {
            var dateRange = new DateRange(new DateTime(2020, 4, 1), new DateTime(2020, 4, 30));
            IList<DateTime> result;
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                result = calendarContext.GetLastBusinessDaysOfWeek(this.calendar.Key, dateRange, 1).ToList();
            }

            Assert.AreEqual(4, result.Count, "Only four weeks end in April");
            Assert.AreEqual(new DateTime(2020, 4, 3), result[0]);
            Assert.AreEqual(new DateTime(2020, 4, 9), result[1], "Good Friday is a holiday");
            Assert.AreEqual(new DateTime(2020, 4, 17), result[2]);
            Assert.AreEqual(new DateTime(2020, 4, 24), result[3]);
        }

        [TestMethod]
        public void Get_First_Working_Day_of_Weeks_In_Apri_and_Mayl()
        {
            var dateRange = new DateRange(new DateTime(2020, 4, 1), new DateTime(2020, 5, 31));
            IList<DateTime> result;
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                result = calendarContext.GetFirstBusinessDaysOfWeek(this.calendar.Key, dateRange, 1).ToList();
            }

            Assert.AreEqual(8, result.Count, "Only eight weeks start in April and May");
            Assert.AreEqual(new DateTime(2020, 4, 6), result[0]);
            Assert.AreEqual(new DateTime(2020, 4, 13), result[1]);
            Assert.AreEqual(new DateTime(2020, 4, 20), result[2]);
            Assert.AreEqual(new DateTime(2020, 4, 27), result[3]);
            Assert.AreEqual(new DateTime(2020, 5, 4), result[4]);
            Assert.AreEqual(new DateTime(2020, 5, 11), result[5]);
            Assert.AreEqual(new DateTime(2020, 5, 18), result[6]);
            Assert.AreEqual(new DateTime(2020, 5, 26), result[7], "Skip Memorial Day");
        }

        [TestMethod]
        public void Get_Last_Working_Day_of_Weeks_In_April_and_May()
        {
            var dateRange = new DateRange(new DateTime(2020, 4, 1), new DateTime(2020, 5, 31));
            IList<DateTime> result;
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                result = calendarContext.GetLastBusinessDaysOfWeek(this.calendar.Key, dateRange, 1).ToList();
            }

            Assert.AreEqual(9, result.Count, "Only nine weeks end in April and May");
            Assert.AreEqual(new DateTime(2020, 4, 3), result[0]);
            Assert.AreEqual(new DateTime(2020, 4, 9), result[1], "Skip Good Friday");
            Assert.AreEqual(new DateTime(2020, 4, 17), result[2]);
            Assert.AreEqual(new DateTime(2020, 4, 24), result[3]);
            Assert.AreEqual(new DateTime(2020, 5, 1), result[4]);
            Assert.AreEqual(new DateTime(2020, 5, 8), result[5]);
            Assert.AreEqual(new DateTime(2020, 5, 15), result[6]);
            Assert.AreEqual(new DateTime(2020, 5, 22), result[7]);
            Assert.AreEqual(new DateTime(2020, 5, 29), result[8]);
        }

        [TestMethod]
        public void Get_Third_Working_Day_of_Weeks_In_May()
        {
            var dateRange = new DateRange(new DateTime(2020, 5, 1), new DateTime(2020, 5, 31));
            IList<DateTime> result;
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                result = calendarContext.GetFirstBusinessDaysOfWeek(this.calendar.Key, dateRange, 3).ToList();
            }

            Assert.AreEqual(4, result.Count, "Only four weeks have third working day in May");
            Assert.AreEqual(new DateTime(2020, 5, 6), result[0]);
            Assert.AreEqual(new DateTime(2020, 5, 13), result[1]);
            Assert.AreEqual(new DateTime(2020, 5, 20), result[2]);
            Assert.AreEqual(new DateTime(2020, 5, 28), result[3], "Skip Memorial Day");
        }

        [TestMethod]
        public void Get_Third_to_Last_Working_Day_of_Weeks_In_May()
        {
            var dateRange = new DateRange(new DateTime(2020, 5, 1), new DateTime(2020, 5, 31));
            IList<DateTime> result;
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                result = calendarContext.GetLastBusinessDaysOfWeek(this.calendar.Key, dateRange, 3).ToList();
            }

            Assert.AreEqual(4, result.Count, "Only four weeks have third last working day in May");
            Assert.AreEqual(new DateTime(2020, 5, 6), result[0]);
            Assert.AreEqual(new DateTime(2020, 5, 13), result[1]);
            Assert.AreEqual(new DateTime(2020, 5, 20), result[2]);
            Assert.AreEqual(new DateTime(2020, 5, 27), result[3]);
        }

        [TestMethod]
        public void Performance_Test_Over_Twenty_Years_Get_First_Day_Of_Each_Week()
        {
            var dateRange = new DateRange(new DateTime(2001, 1, 1), new DateTime(2020, 12, 31));
            IList<DateTime> result;
            using (var calendarContext = new CalendarContext(this.calendar))
            {
                result = calendarContext.GetFirstBusinessDaysOfWeek(this.calendar.Key, dateRange, 1).ToList();
            }

            Assert.AreEqual((int)(20 * 365.25 / 7) + 1, result.Count);
        }

        [TestMethod]
        public void Get_Holidays_For_2020()
        {
            var dateRange = new DateRange(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
            var result = this.calendar.GetHolidays(dateRange);

            Assert.AreEqual(9, result.Count());
        }

        [TestMethod]
        public void Get_Holidays_For_PersonalCalendar()
        {
            var myCalendar = CalendarFactory.CreateFromDateRanges(
                "CAL",
                new[]
                {
                    new DateRange(new DateTime(2020, 1, 1), new DateTime(2020, 1, 1, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 1, 20), new DateTime(2020, 1, 20, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 2, 17), new DateTime(2020, 2, 17, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 4, 6), new DateTime(2020, 4, 10, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 5, 25), new DateTime(2020, 5, 25, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 7, 3), new DateTime(2020, 7, 3, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 8, 10), new DateTime(2020, 8, 21, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 9, 7), new DateTime(2020, 9, 7, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 11, 26), new DateTime(2020, 11, 26, 23, 59, 59)),
                    new DateRange(new DateTime(2020, 12, 25), new DateTime(2020, 12, 25, 23, 59, 59)),
                });

            var dateRange = new DateRange(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
            var result = myCalendar.GetHolidays(dateRange);

            Assert.AreEqual(9 + 4 + 10, result.Count(), "9 Holidays, 4 vacation days prior to Good Friday, 10 vacation days in August.");
        }

        [TestMethod]
        public void Get_Holidays_For_Personal_and_Business_Calendar()
        {
            var dateRange = new DateRange(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
            var compositeCalendar = CalendarFactory.MergeCalendars("__composite", new[] { this.calendar, this.personalCalendar });
            var result = compositeCalendar.GetHolidays(dateRange);

            Assert.AreEqual(9 + 4 + 10, result.Count(), "9 Holidays, 4 vacation days prior to Good Friday, 10 vacation days in August.");
        }

        [TestMethod]
        public void Get_Australian_Holidays_From_Service()
        {
            var dateRange = new DateRange(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
            var asOfDate = new DateTime(2020, 4, 10); // Good Friday
            var service = new CalendarificService();
            using (var context = new CalendarContext(service, dateRange))
            {
                var result = context.GetNextBusinessDay("au", asOfDate);

                Assert.AreEqual(new DateTime(2020, 4, 14), result, "Australia observes Good Monday, so skip to 14th.");
            }
        }

        [TestMethod]
        public void Get_Australian_Holidays_From_Service_Many_Times_On_Multiple_Threads()
        {
            var dateRange = new DateRange(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));
            var asOfDate = new DateTime(2020, 4, 10); // Good Friday
            var service = new CalendarificService();
            using (var context = new CalendarContext(service, dateRange))
            {
                // Make the first call
                context.GetNextBusinessDay("au", asOfDate);

                // Calculate the next business day for every day of the year.
                // We expect all of these to be cached and therefore to return quickly.
                var sw = Stopwatch.StartNew();
                var days = Enumerable.Range(0, 364);
                foreach (var day in days.AsParallel())
                {
                    var date = new DateTime(asOfDate.Year, 1, 1).AddDays(day);
                    var result = context.GetNextBusinessDay("au", date);

                    Assert.IsTrue(result.Subtract(date).TotalDays <= 4, "Australia has no more than a four day break (Easter).");
                }

                Assert.IsTrue(sw.ElapsedMilliseconds < 50, "All subsequent calls even across threads should be using cached calendar and therefore take no more than a few milliseconds.");
            }
        }

        [TestMethod]
        public void Count_Three_Calendar_Days()
        {
            var asOfDate = new DateTime(2020, 6, 4);
            using (var context = new CalendarContext())
            {
                var result = context.AddBusinessDay(CalendarContext.CalendarNames.CalendarDay, asOfDate, 3);
                Assert.AreEqual(new DateTime(2020, 6, 7), result, "Calendar calendar counts weekends");
            }
        }

        [TestMethod]
        public void Count_Three_Business_Days()
        {
            var asOfDate = new DateTime(2020, 6, 4);
            using (var context = new CalendarContext())
            {
                var result = context.AddBusinessDay(CalendarContext.CalendarNames.BusinessDay, asOfDate, 3);
                Assert.AreEqual(new DateTime(2020, 6, 9), result, "Business day calendar skips weekends");
            }
        }

        #region Negative testing

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-3)]
        [DataRow(8)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Get_Business_Day_Of_Week_Expects_Offset_Greater_Than_Zero(int input)
        {
            this.calendar.GetFirstBusinessDaysOfWeek(new DateRange(new DateTime(2020, 1,1 ), new DateTime(2020, 12, 31)), input).Count();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Get_Business_Day_of_Week_Invalid_Date_Range()
        {
            this.calendar.GetFirstBusinessDaysOfWeek(new DateRange(null, new DateTime(2020, 12, 31)), 1).Count();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Invalid_Calendar()
        {
            using (var calendarContext = new CalendarContext(this.calendar, this.personalCalendar))
            {
                calendarContext.AddBusinessDay("WRONG", DateTime.Today, 1);
            }
        }

        #endregion
    }
}
