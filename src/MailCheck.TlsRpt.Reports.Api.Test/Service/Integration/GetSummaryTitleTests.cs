using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Reports.Api.Config;
using MailCheck.TlsRpt.Reports.Api.Dao;
using MailCheck.TlsRpt.Reports.Api.Service;
using MailCheck.TlsRpt.Reports.Contracts;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Api.Test.Service.Integration
{
    [TestFixture(Category = "IntegrationCI")]
    public class GetSummaryTitleTests : BaseIntegrationTest
    {
        private IDocumentDbConfig _config;
        private ILogger<ReportsApiDao> _logger;
        private IClock _clock;
        private ReportService _service;

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger<ReportsApiDao>>();

            _clock = A.Fake<IClock>();

            _config = A.Fake<IDocumentDbConfig>();
            _config.Database = "test";

            ReportsApiDao reportsApiDao = new ReportsApiDao(_config, _logger, _clock, new MongoClientProvider(_config, Runner.ConnectionString));

            _service = new ReportService(reportsApiDao, _clock);
        }

        [Test]
        public async Task GetSummaryTitleCalculatesCorrectPercentages()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                // 91 days ago
                CreateReport("microsoft.com", startDate: "2020-10-02T00:00:00Z", endDate:"2020-10-02T23:59:59Z", successfulCount: 1000, negotiationFailureCount: 1),

                // 90 days ago
                CreateReport("mailchimp.com", startDate: "2020-10-03T00:00:00Z", endDate:"2020-10-03T23:59:59Z", successfulCount: 40, negotiationFailureCount: 60),

                // 15 days ago
                CreateReport("microsoft.com", startDate: "2020-12-17T00:00:00Z", endDate:"2020-12-17T23:59:59Z", successfulCount: 50, negotiationFailureCount: 50),

                // 14 days ago
                CreateReport("microsoft.com", startDate: "2020-12-18T00:00:00Z", endDate:"2020-12-18T23:59:59Z", successfulCount: 60, negotiationFailureCount: 40),

                // 13 days ago
                CreateReport("microsoft.com", startDate: "2020-12-19T00:00:00Z", endDate:"2020-12-19T23:59:59Z", successfulCount: 70, negotiationFailureCount: 30),
                
                // 2 days ago
                CreateReport("microsoft.com", startDate: "2020-12-30T00:00:00Z", endDate:"2020-12-30T23:59:59Z", successfulCount: 80, negotiationFailureCount: 20),

                // 1 day ago
                CreateReport("google.com",    startDate: "2020-12-31T00:00:00Z", endDate:"2020-12-31T23:59:59Z", successfulCount: 90, negotiationFailureCount: 10),
                CreateReport("mailchimp.com", startDate: "2020-12-31T00:00:00Z", endDate:"2020-12-31T23:59:59Z", successfulCount: 100, negotiationFailureCount: 0),
            };

            StoreReports(reports);

            List<TitleResult> result = await _service.GetSummaryTitle("testReportDomain");

            Assert.AreEqual(3, result.Count);

            // In the last 2 days, 2 providers reported TLS failures accounting for 10% of all inbound sessions
            Assert.AreEqual(2, result[0].Period);
            Assert.AreEqual(2, result[0].ProviderCount);
            Assert.AreEqual(10.0d, result[0].PercentageFailures);

            // In the last 14 days, 2 providers reported TLS failures accounting for 20% of all inbound sessions
            Assert.AreEqual(14, result[1].Period);
            Assert.AreEqual(2, result[1].ProviderCount);
            Assert.AreEqual(20.0d, result[1].PercentageFailures);

            // In the last 90 days, 3 providers reported TLS failures accounting for 29.96% of all inbound sessions
            Assert.AreEqual(90, result[2].Period);
            Assert.AreEqual(3, result[2].ProviderCount);
            Assert.AreEqual(30.0d, result[2].PercentageFailures);
        }

        [Test]
        public async Task GetSummaryTitleCalculatesFromMidnight()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                CreateReport("google.com", startDate: "2020-12-29T00:00:00Z", endDate:"2020-12-30T00:00:01Z", negotiationFailureCount: 1),
            };

            StoreReports(reports);

            List<TitleResult> result = await _service.GetSummaryTitle("testReportDomain");

            Assert.AreEqual(1, result[0].ProviderCount);
            Assert.AreEqual(1, result[1].ProviderCount);
            Assert.AreEqual(1, result[2].ProviderCount);
        }

        [Test]
        public async Task GetSummaryTitleOmitsReportsEndingOnSameDayAsRequest()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                CreateReport("google.com", startDate: "2020-12-30T00:00:00Z", endDate:"2021-01-01T09:00:00Z", negotiationFailureCount: 1),
            };

            StoreReports(reports);

            List<TitleResult> result = await _service.GetSummaryTitle("testReportDomain");

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task GetSummaryTitleReturnsResultsWhenSuccessfulData()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));
            List<ReportInfo> reports = new List<ReportInfo>
            {
                CreateReport("google.com",    startDate: "2020-12-31T00:00:00Z", endDate:"2020-12-31T23:59:59Z", successfulCount: 123),
            };
            StoreReports(reports);

            List<TitleResult> result = await _service.GetSummaryTitle("testReportDomain");

            Assert.AreEqual(3, result.Count);

            Assert.AreEqual(2, result[0].Period);
            Assert.AreEqual(0, result[0].ProviderCount);
            Assert.AreEqual(0d, result[0].PercentageFailures);

            Assert.AreEqual(14, result[1].Period);
            Assert.AreEqual(0, result[1].ProviderCount);
            Assert.AreEqual(0d, result[1].PercentageFailures);

            Assert.AreEqual(90, result[2].Period);
            Assert.AreEqual(0, result[2].ProviderCount);
            Assert.AreEqual(0d, result[2].PercentageFailures);
        }

        [Test]
        public async Task GetSummaryTitleReturnsEmptyResultsWhenNoData()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            List<TitleResult> result = await _service.GetSummaryTitle("testReportDomain");

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task GetSummaryTitleReturnsEmptyResultsWhen90DayDataOnly()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                // 90 days ago
                CreateReport("mailchimp.com", startDate: "2020-10-03T00:00:00Z", endDate:"2020-10-03T23:59:59Z", negotiationFailureCount: 1),
            };

            StoreReports(reports);

            List<TitleResult> result = await _service.GetSummaryTitle("testReportDomain");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(90, result[0].Period);
        }

        [Test]
        public async Task GetSummaryTitleReturnsEmptyResultsWhen90And14DayData()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                // 14 days ago
                CreateReport("microsoft.com", startDate: "2020-12-18T00:00:00Z", endDate:"2020-12-18T23:59:59Z", negotiationFailureCount: 1),
            };

            StoreReports(reports);

            List<TitleResult> result = await _service.GetSummaryTitle("testReportDomain");

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(14, result[0].Period);
            Assert.AreEqual(1, result[0].ProviderCount);
            Assert.AreEqual(100d, result[0].PercentageFailures);

            Assert.AreEqual(90, result[1].Period);
            Assert.AreEqual(1, result[1].ProviderCount);
            Assert.AreEqual(100d, result[1].PercentageFailures);
        }
    }
}