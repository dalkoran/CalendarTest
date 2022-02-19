namespace Spencen.Common.Calendar.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RelativeDateBuilderTests
    {
        [TestMethod]
        public void MoveToBeginningOfYear()
        {
            var expected = new DateTime(DateTime.Today.Year, 1, 1);
            var sut = new RelativeDateBuilder()
                .MoveToBeginningOf(DateTimeInterval.Year)
                .Create();

            var result = sut.Apply(DateTime.Today);

            Assert.AreEqual(expected, result, sut.Description);
        }

        [TestMethod]
        public void MoveToBeginningOfYearThenAddThreeMonths()
        {
            var expected = new DateTime(DateTime.Today.Year, 4, 1);
            var sut = new RelativeDateBuilder()
                .MoveToBeginningOf(DateTimeInterval.Year)
                .Add(DateTimeInterval.Month, 3)
                .Create();

            var result = sut.Apply(DateTime.Today);

            Assert.AreEqual(expected, result, sut.Description);
        }

        [TestMethod]
        public void MoveToBeginningOfFourthOfJuly()
        {
            var expected = new DateTime(DateTime.Today.Year, 7, 4);
            var sut = new RelativeDateBuilder()
                .MoveTo(DateTimeInterval.Month, 7)
                .MoveTo(DateTimeInterval.Day, 4)
                .Create();

            var result = sut.Apply(DateTime.Today);

            Assert.AreEqual(expected, result, sut.Description);
        }

        [TestMethod]
        public void MoveToBeginningJulyThenFourthPriorWeekDay()
        {
            var now = new DateTime(2020, 5, 5);
            var expected = new DateTime(now.Year, 6, 25);
            var sut = new RelativeDateBuilder()
                .MoveTo(DateTimeInterval.Month, 7)
                .MoveToBeginningOf(DateTimeInterval.Month)
                .Subtract(DateTimeInterval.WeekDay, 4)
                .Create();

            var result = sut.Apply(now);

            Assert.AreEqual(expected, result, sut.Description);
        }

        [TestMethod]
        public void MoveToNovemberThenForwardToFourthThursday()
        {
            var now = new DateTime(2020, 5, 5);
            var expected = new DateTime(now.Year, 11, 26, now.Hour, now.Minute, now.Second, now.Millisecond);
            var sut = new RelativeDateBuilder()
                .MoveTo(DateTimeInterval.Month, 11)
                .MoveTo(DayOfWeek.Thursday, 4)
                .Create();

            var result = sut.Apply(now);

            Assert.AreEqual(expected, result, sut.Description);
        }


        [TestMethod]
        public void MoveToLastFridayInNovember()
        {
            var now = new DateTime(2020, 5, 5);
            var expected = new DateTime(now.Year, 11, 27, now.Hour, now.Minute, now.Second, now.Millisecond);
            var sut = new RelativeDateBuilder()
                .MoveTo(DateTimeInterval.Month, 11)
                .MoveToLast(DayOfWeek.Friday)
                .Create();

            var result = sut.Apply(now);

            Assert.AreEqual(expected, result, sut.Description);
        }

        [TestMethod]
        public void JulyFouthHolidayMovesFromSaturdayToFriday()
        {
            var now = new DateTime(2021, 1, 1); // July fourth on a Sunday
            var expected = new DateTime(now.Year, 7, 5, now.Hour, now.Minute, now.Second, now.Millisecond);
            var sut = new RelativeDateBuilder()
                .MoveTo(DateTimeInterval.Month, 7)
                .MoveTo(DateTimeInterval.Day, 4)
                .If(DateTimeInterval.Day, asOf => asOf.DayOfWeek == DayOfWeek.Saturday)
                    .Then(builder => builder.Subtract(DateTimeInterval.Day))
                    .End()
                .If(DateTimeInterval.Day, asOf => asOf.DayOfWeek == DayOfWeek.Sunday)
                    .Then(builder => builder.Add(DateTimeInterval.Day))
                    .End()
                .Create();
            var result = sut.Apply(now);

            Assert.AreEqual(expected, result, sut.Description);
        }

        [TestMethod]
        public void JulyFourthHolidayMovesFromSundayToMonday()
        {
            var now = new DateTime(2020, 1, 1); // July fourth on a Saturday
            var expected = new DateTime(now.Year, 7, 3, now.Hour, now.Minute, now.Second, now.Millisecond);
            var sut = new RelativeDateBuilder()
                .MoveTo(DateTimeInterval.Month, 7)
                .MoveTo(DateTimeInterval.Day, 4)
                .If(DateTimeInterval.Day, asOf => asOf.DayOfWeek == DayOfWeek.Saturday)
                    .Then(builder => builder.Subtract(DateTimeInterval.Day))
                    .End()
                .Create();

            var result = sut.Apply(now);

            Assert.AreEqual(expected, result, sut.Description);
        }
    }
}
