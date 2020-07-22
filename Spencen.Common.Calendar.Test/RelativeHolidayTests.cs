namespace Spencen.Common.Calendar.Test
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RelativeHolidayTests
    {
        [DataTestMethod]
        [DataRow("2000-01-03", 2000)]
        [DataRow("2013-01-01", 2013)]
        [DataRow("2014-01-01", 2014)]
        [DataRow("2015-01-01", 2015)]
        [DataRow("2016-01-01", 2016)]
        [DataRow("2017-01-02", 2017)]
        [DataRow("2018-01-01", 2018)]
        [DataRow("2019-01-01", 2019)]
        [DataRow("2020-01-01", 2020)]
        public void NewsYearsDay(string expectedHoliday, int year)
        {
            var expected = DateTime.Parse(expectedHoliday);
            var result = new DateTime(year, 1, 1).RelativeDate("^y>D");

            Assert.AreEqual(expected, result, $"New Years Day holiday {year}");
        }

        [DataTestMethod]
        [DataRow("2016-07-04", 2016)]
        [DataRow("2017-07-04", 2017)]
        [DataRow("2018-07-04", 2018)]
        [DataRow("2019-07-04", 2019)]
        [DataRow("2020-07-03", 2020)]
        [DataRow("2021-07-05", 2021)]
        [DataRow("2022-07-04", 2022)]
        public void FourthOfJuly(string expectedHoliday, int year)
        {
            var expected = DateTime.Parse(expectedHoliday);
            var result = new DateTime(year, 1, 1).RelativeDate("@7M@4d?sat{<D}?sun{>D}");

            Assert.AreEqual(expected, result, $"Fourth of July (Observed) {year}");
        }

        [DataTestMethod]
        [DataRow("2016-11-24", 2016)]
        [DataRow("2017-11-23", 2017)]
        [DataRow("2018-11-22", 2018)]
        [DataRow("2019-11-28", 2019)]
        [DataRow("2020-11-26", 2020)]
        [DataRow("2021-11-25", 2021)]
        [DataRow("2022-11-24", 2022)]
        public void Thanksgiving(string expectedHoliday, int year)
        {
            var expected = DateTime.Parse(expectedHoliday);
            var result = new DateTime(year, 1, 1).RelativeDate("@11M@4thu");

            Assert.AreEqual(expected, result, $"Thanksgiving {year}");
        }
    }
}
