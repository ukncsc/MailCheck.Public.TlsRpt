using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Reports.Api.Config;
using MailCheck.TlsRpt.Reports.Api.Dao;
using MailCheck.TlsRpt.Reports.Api.Service;
using MailCheck.TlsRpt.Reports.Contracts;
using MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Api.Test.Service.Integration
{
    [TestFixture(Category = "IntegrationCI")]
    public class GetSummaryBodyTests : BaseIntegrationTest
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
        public async Task GetSummaryBodyGroupsByProviderAndType()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-02T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                CreateReport("google.com", startDate: "2020-12-19T00:00:00Z", endDate:"2020-12-19T23:59:59Z", successfulCount: 97, negotiationFailureCount: 0, policyFailureCount: 1, generalFailureCount: 2),
                CreateReport("google.com", startDate: "2021-01-01T00:00:00Z", endDate:"2021-01-01T23:59:59Z", successfulCount: 93, negotiationFailureCount: 0, policyFailureCount: 3, generalFailureCount: 4),

                CreateReport("microsoft.com", startDate: "2020-12-19T00:00:00Z", endDate:"2020-12-19T23:59:59Z", successfulCount: 79, negotiationFailureCount: 6, policyFailureCount: 7, generalFailureCount: 8),
                CreateReport("microsoft.com", startDate: "2021-01-01T00:00:00Z", endDate:"2021-01-01T23:59:59Z", successfulCount: 70, negotiationFailureCount: 9, policyFailureCount: 10, generalFailureCount: 11),
            };

            StoreReports(reports);

            List<ProviderFailure> result = await _service.GetSummaryBody("testReportDomain", 14);

            Assert.AreEqual(5, result.Count);

            Assert.AreEqual("google.com", result[0].Provider);
            Assert.AreEqual(3, result[0].Percent);
            Assert.AreEqual(ResultType.StsPolicyInvalid, result[0].ResultType);

            Assert.AreEqual("google.com", result[1].Provider);
            Assert.AreEqual(2, result[1].Percent);
            Assert.AreEqual(ResultType.DaneRequired, result[1].ResultType);

            Assert.AreEqual("microsoft.com", result[2].Provider);
            Assert.AreEqual(9.5, result[2].Percent);
            Assert.AreEqual(ResultType.StsPolicyInvalid, result[2].ResultType);

            Assert.AreEqual("microsoft.com", result[3].Provider);
            Assert.AreEqual(8.5, result[3].Percent);
            Assert.AreEqual(ResultType.DaneRequired, result[3].ResultType);

            Assert.AreEqual("microsoft.com", result[4].Provider);
            Assert.AreEqual(7.5, result[4].Percent);
            Assert.AreEqual(ResultType.StartTlsNotSupported, result[4].ResultType);
        }

        [Test]
        public async Task GetSummaryBodyOnlyShowsFailures()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-02T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                CreateReport("google.com",    startDate: "2021-01-01T00:00:00Z", endDate:"2021-01-01T23:59:59Z", successfulCount: 100),
                CreateReport("microsoft.com", startDate: "2021-01-01T00:00:00Z", endDate:"2021-01-01T23:59:59Z", successfulCount: 200),
            };

            StoreReports(reports);

            List<ProviderFailure> result = await _service.GetSummaryBody("testReportDomain", 1);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task GetSummaryBodyPercentagesRounded()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                CreateReport("google.com", startDate: "2020-12-31T00:00:00Z", endDate:"2020-12-31T23:59:59Z", successfulCount: 100, negotiationFailureCount: 0, policyFailureCount: 50, generalFailureCount: 0),
            };

            StoreReports(reports);

            List<ProviderFailure> result = await _service.GetSummaryBody("testReportDomain", 1);

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual("google.com", result[0].Provider);
            Assert.AreEqual(33.33, result[0].Percent);
            Assert.AreEqual(ResultType.DaneRequired, result[0].ResultType);
        }

        [Test]
        public async Task GetSummaryBodyIncludesMultipleReportsInDay()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                CreateReport("google.com", startDate: "2020-12-31T00:00:00Z", endDate:"2020-12-31T23:59:59Z", successfulCount: 99, negotiationFailureCount: 1),
                CreateReport("google.com", startDate: "2020-12-31T00:00:00Z", endDate:"2020-12-31T23:59:59Z", successfulCount: 98, negotiationFailureCount: 2)
            };

            StoreReports(reports);

            List<ProviderFailure> result = await _service.GetSummaryBody("testReportDomain", 1);

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual("google.com", result[0].Provider);
            Assert.AreEqual(1.5, result[0].Percent);
            Assert.AreEqual(ResultType.StartTlsNotSupported, result[0].ResultType);
        }

        [Test]
        public async Task GetSummaryBodyCalculatesFromMidnight()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                CreateReport("google.com", startDate: "2020-12-30T00:00:00Z", endDate:"2020-12-31T00:00:01Z", negotiationFailureCount: 1),
            };

            StoreReports(reports);

            List<ProviderFailure> result = await _service.GetSummaryBody("testReportDomain", 1);

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public async Task GetSummaryDetailsOmitsReportsEndingOnSameDayAsRequest()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                CreateReport("google.com", startDate: "2020-12-30T00:00:00Z", endDate:"2021-01-01T09:00:00Z", negotiationFailureCount: 1),
            };

            StoreReports(reports);

            List<ProviderFailure> result = await _service.GetSummaryBody("testReportDomain", 1);

            Assert.AreEqual(0, result.Count);
        }
    }
}