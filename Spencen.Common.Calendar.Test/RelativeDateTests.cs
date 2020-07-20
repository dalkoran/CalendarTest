namespace Spencen.Common.Calendar.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RelativeDateTests
    {
        [TestMethod]
        public void StartOfThisMonth()
        {
            var result = DateTime.Now.RelativeDate("^M");
            var expected = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void StartOfThisYear()
        {
            var result = DateTime.Now.RelativeDate("^y");
            var expected = new DateTime(DateTime.Today.Year, 1, 1);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void StartOfThisDay()
        {
            var result = DateTime.Now.RelativeDate("^d");
            var expected = DateTime.Today;
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TheYear2019()
        {
            var result = DateTime.Now.RelativeDate("@2019y");
            Assert.AreEqual(2019, result.Year);
        }

        [TestMethod]
        public void EigthMonth()
        {
            var result = DateTime.Now.RelativeDate("@8M");
            Assert.AreEqual(8, result.Month);
        }

        [TestMethod]
        public void FourteenthDay()
        {
            var result = DateTime.Now.RelativeDate("@14d");
            Assert.AreEqual(14, result.Day);
        }

        [TestMethod]
        public void Add3Years()
        {
            var now = DateTime.Now;
            var result = now.RelativeDate("+3y");
            var expected = now.AddYears(3);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Subtract14Months()
        {
            var now = DateTime.Now;
            var result = now.RelativeDate("-14M");
            var expected = now.AddMonths(-14);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Add25Days()
        {
            var now = DateTime.Now;
            var result = now.RelativeDate("+25d");
            var expected = now.AddDays(25);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CompositeAddSubtract()
        {
            var now = DateTime.Now;
            var result = now.RelativeDate("+23y-23M+23S-8m");
            var expected = now.AddYears(23).AddMonths(-23).AddMilliseconds(23).AddMinutes(-8);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AbsoluteTimeCurrentDay()
        {
            var now = DateTime.Now;
            var result = now.RelativeDate("^d+7H");
            var expected = now.Date.AddHours(7);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AddThreeWeekdays()
        {
            var now = new DateTime(2020, 07, 16);
            var result = now.RelativeDate("+3D");
            var expected = new DateTime(2020, 7, 21);
            Assert.AreEqual(expected, result);
        }


        [TestMethod]
        public void AddFourWeekendDays()
        {
            var now = new DateTime(2020, 07, 16);
            var result = now.RelativeDate("+4e");
            var expected = new DateTime(2020, 7, 26);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void March22atEightThirtyCurrentYear()
        {
            var now = DateTime.Now;
            var result = now.RelativeDate("@3M@22d@8H@30m@0s@0S"); // (2M)22d (8H)30m
            var expected = new DateTime(now.Year, 3, 22, 8, 30, 0);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void MoveForwardToSaturday()
        {
            var now = new DateTime(2020, 07, 19);
            var result = now.RelativeDate(">sat"); 
            var expected = new DateTime(2020, 7, 25);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void MoveForwardThreeSaturdaysThenSkipToNovember()
        {
            var now = new DateTime(2020, 07, 19);
            var result = now.RelativeDate(">3sat>nov");
            var expected = new DateTime(2020, 11, 8);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void FourthMondayInCurrentMonth()
        {
            var now = new DateTime(2020, 07, 19);
            var result = now.RelativeDate("^M>4mon");
            var expected = new DateTime(2020, 7, 27);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void FourthMondayFromBeginningOfNextMonth()
        {
            var now = new DateTime(2020, 07, 19);
            var result = now.RelativeDate("+M^M>4mon");
            var expected = new DateTime(2020, 8, 24);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void BeginningOfTodayAdvanceSevenHours()
        {
            var now = DateTime.Now;
            var result = now.RelativeDate("^d+7H");
            var expected = now.Date.AddHours(7);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void MoveBackwardFourWeekendDays()
        {
            var now = new DateTime(2020, 07, 19);
            var result = now.RelativeDate("<4e");
            var expected = new DateTime(2020, 7, 5);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void FirstFridayOfCurrentMonth()
        {
            var now = new DateTime(2020, 07, 19);
            var result = now.RelativeDate("@fri");
            var expected = new DateTime(2020, 7, 3);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void SecondFridayOfCurrentMonth()
        {
            var now = new DateTime(2020, 07, 19);
            var result = now.RelativeDate("@2fri");
            var expected = new DateTime(2020, 7, 10);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void LastFridayOfCurrentMonth()
        {
            var now = new DateTime(2020, 08, 19);
            var result = now.RelativeDate("@!fri");
            var expected = new DateTime(2020, 8, 28);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void SecondLastFridayOfCurrentMonth()
        {
            var now = new DateTime(2020, 08, 19);
            var result = now.RelativeDate("@2!fri");
            var expected = new DateTime(2020, 8, 21);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void LastMondayOfCurrentMonth()
        {
            var now = new DateTime(2020, 08, 19);
            var result = now.RelativeDate("@!mon");
            var expected = new DateTime(2020, 8, 31);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void SecondLastMondayOfCurrentMonth()
        {
            var now = new DateTime(2020, 08, 19);
            var result = now.RelativeDate("@2!mon");
            var expected = new DateTime(2020, 8, 24);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void LastWednesdayOfCurrentMonth()
        {
            var now = new DateTime(2020, 08, 19);
            var result = now.RelativeDate("@!wed");
            var expected = new DateTime(2020, 8, 26);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void SecondLastWednesdayOfCurrentMonth()
        {
            var now = new DateTime(2020, 08, 19);
            var result = now.RelativeDate("@2!wed");
            var expected = new DateTime(2020, 8, 19);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void MoveSaturdayToFriday()
        {
            var now = new DateTime(2020, 07, 18);
            var result = now.RelativeDate("?sat{-d}?sun{+d}");
            var expected = new DateTime(2020, 7, 17);
            Assert.AreEqual(expected, result);
        }


        [TestMethod]
        public void MoveSundayToMonday()
        {
            var now = new DateTime(2020, 07, 19);
            var result = now.RelativeDate("?sat{-d}?sun{+d}");
            var expected = new DateTime(2020, 7, 20);
            Assert.AreEqual(expected, result);
        }


        [TestMethod]
        public void DoNotMoveWeekday()
        {
            var now = new DateTime(2020, 07, 17);
            var result = now.RelativeDate("?sat{-d}?sun{+d}");
            var expected = new DateTime(2020, 7, 17);
            Assert.AreEqual(expected, result);
        }
    }
}
