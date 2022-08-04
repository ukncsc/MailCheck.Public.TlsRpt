using System;
using System.Collections.Generic;
using FakeItEasy;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Reports.Entity.Dao;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Entity.Test.Dao
{
    [TestFixture]
    public class ReportPeriodCalculatorTests
    {
        private IClock _clock;
        private IReportPeriodCalculator _reportInfoFilter;

        [SetUp]
        public void SetUp()
        {
            _clock = A.Fake<IClock>();
            _reportInfoFilter = new ReportPeriodCalculator(_clock);
        }

        [TestCaseSource(nameof(DateRangeTestCases))]
        public void GetDateRange(DateRangeTestCase testCase)
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(testCase.Now);

            var (actualStart, actualEnd) = _reportInfoFilter.GetDateRangesForPeriods(new[] { testCase.Period })[testCase.Period];

            Assert.That(actualStart, Is.EqualTo(testCase.ExpectedStart));
            Assert.That(actualEnd, Is.EqualTo(testCase.ExpectedEnd));
        }

        static IEnumerable<DateRangeTestCase> DateRangeTestCases()
        {
            yield return new DateRangeTestCase
            {
                Description = "In the middle of the day on the 16th the 2-day period I'm looking for is 13th - 14th inclusive",
                Now = new DateTime(2021, 5, 16, 13, 14, 15, DateTimeKind.Utc),
                Period = 2,
                ExpectedStart = new DateTime(2021, 5, 13, 0, 0, 0, DateTimeKind.Utc),
                ExpectedEnd = new DateTime(2021, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            };
            yield return new DateRangeTestCase
            {
                Description = "At midnight on the 16th the 2-day period I'm looking for is 13th - 14th inclusive",
                Now = new DateTime(2021, 5, 16, 00, 00, 00, DateTimeKind.Utc),
                Period = 2,
                ExpectedStart = new DateTime(2021, 5, 13, 0, 0, 0, DateTimeKind.Utc),
                ExpectedEnd = new DateTime(2021, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            };
            yield return new DateRangeTestCase
            {
                Description = "At the end of the 16th the 2-day period I'm looking for is 13th - 14th inclusive",
                Now = new DateTime(2021, 5, 16, 23, 59, 59, DateTimeKind.Utc),
                Period = 2,
                ExpectedStart = new DateTime(2021, 5, 13, 0, 0, 0, DateTimeKind.Utc),
                ExpectedEnd = new DateTime(2021, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            };
            yield return new DateRangeTestCase
            {
                Description = "In the middle of the day on the 16th the 14-day period I'm looking for is 1st - 14th inclusive",
                Now = new DateTime(2021, 5, 15, 13, 14, 15, DateTimeKind.Utc),
                Period = 14,
                ExpectedStart = new DateTime(2021, 4, 30, 0, 0, 0, DateTimeKind.Utc),
                ExpectedEnd = new DateTime(2021, 5, 14, 0, 0, 0, DateTimeKind.Utc),
            };
        }
    }

    public class DateRangeTestCase
    {
        public string Description { get; set; }

        public DateTime Now { get; set; }

        public int Period { get; set; }

        public DateTime ExpectedStart { get; set; }

        public DateTime ExpectedEnd { get; set; }
    }
}
