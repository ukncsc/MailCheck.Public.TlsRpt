using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Reports.Api.Dao;
using MailCheck.TlsRpt.Reports.Api.Service;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Api.Test.Service
{
    [TestFixture]
    public class ReportServiceTests
    {
        private ReportService _reportService;
        private IReportsApiDao _reportsApiDao;
        private IClock _clock;
        private readonly DateTime _defaultTestDate = new DateTime(2000, 01, 01);

        [SetUp]
        public void SetUp()
        {
            _reportsApiDao = A.Fake<IReportsApiDao>();
            _clock = A.Fake<IClock>();
            _reportService = new ReportService(_reportsApiDao, _clock);
        }

        private ProviderTotals CreateProviderTotals(string provider, int period, int totalSuccessfulSessionCount, int totalFailureSessionCount)
        {
            return new ProviderTotals
            {
                ReportEndDate = _defaultTestDate.AddDays(-period).AddTicks(1),
                TotalSuccessfulSessionCount = totalSuccessfulSessionCount,
                TotalFailureSessionCount = totalFailureSessionCount,
                Provider = provider
            };
        }

        [Test]
        public async Task GetSummaryTitleReturnsTitlesForThreePeriods()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(_defaultTestDate);

            A.CallTo(() => _reportsApiDao.GetTotals("testDomain", 90)).Returns(new List<ProviderTotals>
            {
                CreateProviderTotals("testProvider1", 01, 10, 20),

                CreateProviderTotals("testProvider1", 14, 30, 40),
                CreateProviderTotals("testProvider2", 14, 50, 60),

                CreateProviderTotals("testProvider1", 90, 70, 80),
                CreateProviderTotals("testProvider2", 90, 90, 100),
                CreateProviderTotals("testProvider3", 90, 110, 120)
            });

            List<TitleResult> result = await _reportService.GetSummaryTitle("testDomain");

            Assert.AreEqual(2, result[0].Period);
            Assert.AreEqual(1, result[0].ProviderCount);
            Assert.AreEqual(66.67, result[0].PercentageFailures);

            Assert.AreEqual(14, result[1].Period);
            Assert.AreEqual(2, result[1].ProviderCount);
            Assert.AreEqual(57.14, result[1].PercentageFailures);

            Assert.AreEqual(90, result[2].Period);
            Assert.AreEqual(3, result[2].ProviderCount);
            Assert.AreEqual(53.85, result[2].PercentageFailures);
        }

        [Test]
        public async Task GetSummaryTitleCorrectWhenNoFailures()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(new DateTime(2000, 01, 01));

            A.CallTo(() => _reportsApiDao.GetTotals("testDomain", 90)).Returns(new List<ProviderTotals>
            {
                CreateProviderTotals("testProvider1", 1, 100, 0)
            });

            List<TitleResult> result = await _reportService.GetSummaryTitle("testDomain");

            Assert.AreEqual(2, result[0].Period);
            Assert.AreEqual(0, result[0].ProviderCount);
            Assert.AreEqual(0, result[0].PercentageFailures);

            Assert.AreEqual(14, result[1].Period);
            Assert.AreEqual(0, result[1].ProviderCount);
            Assert.AreEqual(0, result[1].PercentageFailures);

            Assert.AreEqual(90, result[2].Period);
            Assert.AreEqual(0, result[2].ProviderCount);
            Assert.AreEqual(0, result[2].PercentageFailures);
        }

        [Test]
        public async Task GetSummaryTitleCorrectWhenNoSessions()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(new DateTime(2000, 01, 01));

            A.CallTo(() => _reportsApiDao.GetTotals("testDomain", 90)).Returns(new List<ProviderTotals>
            {
                CreateProviderTotals("testProvider1", 1, 0, 0)
            });

            List<TitleResult> result = await _reportService.GetSummaryTitle("testDomain");

            Assert.AreEqual(2, result[0].Period);
            Assert.AreEqual(0, result[0].ProviderCount);
            Assert.AreEqual(0, result[0].PercentageFailures);

            Assert.AreEqual(14, result[1].Period);
            Assert.AreEqual(0, result[1].ProviderCount);
            Assert.AreEqual(0, result[1].PercentageFailures);

            Assert.AreEqual(90, result[2].Period);
            Assert.AreEqual(0, result[2].ProviderCount);
            Assert.AreEqual(0, result[2].PercentageFailures);
        }

        [Test]
        public async Task GetSummaryTitleEmptyWhenNoProviderTotals()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(new DateTime(2000, 01, 01));

            A.CallTo(() => _reportsApiDao.GetTotals("testDomain", 90)).Returns(new List<ProviderTotals>());

            List<TitleResult> result = await _reportService.GetSummaryTitle("testDomain");

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task GetSummaryTitleCountsProvidersWithFailuresOnly()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(new DateTime(2000, 01, 01));

            A.CallTo(() => _reportsApiDao.GetTotals("testDomain", 90)).Returns(new List<ProviderTotals>
            {
                CreateProviderTotals("testProvider1", 1, 100, 0),
                CreateProviderTotals("testProvider2", 1, 0, 100)
            });

            List<TitleResult> result = await _reportService.GetSummaryTitle("testDomain");

            Assert.AreEqual(2, result[0].Period);
            Assert.AreEqual(1, result[0].ProviderCount);
            Assert.AreEqual(50.0d, result[0].PercentageFailures);

            Assert.AreEqual(14, result[1].Period);
            Assert.AreEqual(1, result[1].ProviderCount);
            Assert.AreEqual(50.0d, result[1].PercentageFailures);

            Assert.AreEqual(90, result[2].Period);
            Assert.AreEqual(1, result[2].ProviderCount);
            Assert.AreEqual(50.0d, result[2].PercentageFailures);
        }

        [Test]
        public async Task GetSummaryTitleRoundsPercentagesToTwoDecimalPlaces()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(new DateTime(2000, 01, 01));

            A.CallTo(() => _reportsApiDao.GetTotals("testDomain", 90)).Returns(new List<ProviderTotals>
            {
                CreateProviderTotals("testProvider1", 1, 100, 50)
            });

            List<TitleResult> result = await _reportService.GetSummaryTitle("testDomain");

            Assert.AreEqual(2, result[0].Period);
            Assert.AreEqual(1, result[0].ProviderCount);
            Assert.AreEqual(33.33d, result[0].PercentageFailures);
        }
    }
}
