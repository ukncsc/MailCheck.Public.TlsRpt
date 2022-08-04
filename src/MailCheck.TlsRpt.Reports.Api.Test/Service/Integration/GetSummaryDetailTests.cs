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
    public class GetSummaryDetailsTests : BaseIntegrationTest
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
        public async Task GetSummaryDetailsIsGroupedByDateAndProvider2Days()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                CreateReport("google.com", startDate: "2020-12-30T00:00:00Z", endDate:"2020-12-30T23:59:59Z", successfulCount: 100, negotiationFailureCount: 50, policyFailureCount: 50, generalFailureCount: 0),
                CreateReport("microsoft.com", startDate: "2020-12-31T00:00:00Z", endDate:"2020-12-31T23:59:59Z",  policyFailureCount: 0, generalFailureCount: 50),
            };

            StoreReports(reports);

            List<ReportLine> result = await _service.GetSummaryDetails("testReportDomain", 2);

            Assert.AreEqual(3, result.Count);

            Assert.AreEqual("google.com", result[0].OrganizationName);
            Assert.AreEqual("2020-12-30", result[0].Date);
            Assert.AreEqual(50, result[0].FailedSessionCount);
            Assert.AreEqual(100, result[0].TotalFailureSessionCount);
            Assert.AreEqual(100, result[0].TotalSuccessfulSessionCount);
            Assert.AreEqual("DaneRequired", result[0].ResultType);

            Assert.AreEqual("google.com", result[1].OrganizationName);
            Assert.AreEqual("2020-12-30", result[1].Date);
            Assert.AreEqual(50, result[1].FailedSessionCount);
            Assert.AreEqual(100, result[1].TotalFailureSessionCount);
            Assert.AreEqual(100, result[1].TotalSuccessfulSessionCount);
            Assert.AreEqual("StartTlsNotSupported", result[1].ResultType);

            Assert.AreEqual("microsoft.com", result[2].OrganizationName);
            Assert.AreEqual("2020-12-31", result[2].Date);
            Assert.AreEqual(50, result[2].FailedSessionCount);
            Assert.AreEqual(50, result[2].TotalFailureSessionCount);
            Assert.AreEqual(0, result[2].TotalSuccessfulSessionCount);
            Assert.AreEqual("StsPolicyInvalid", result[2].ResultType);
        }

        [Test]
        public async Task GetSummaryDetailsIsGroupedByDateAndProvider14Days()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                // 15 days ago
                CreateReport("mimecast.com", startDate: "2020-12-17T00:00:00Z", endDate:"2020-12-17T23:59:59Z", successfulCount: 1000),

                // 14 days ago
                CreateReport("microsoft.com", startDate: "2020-12-18T00:00:00Z", endDate:"2020-12-18T23:59:59Z", successfulCount: 1, negotiationFailureCount: 0, policyFailureCount: 0, generalFailureCount: 0),

                // 1 day ago
                CreateReport("google.com",    startDate: "2020-12-31T00:00:00Z", endDate:"2020-12-31T23:59:59Z", successfulCount: 2, negotiationFailureCount: 3, policyFailureCount: 4, generalFailureCount: 0),
                CreateReport("microsoft.com", startDate: "2020-12-31T00:00:00Z", endDate:"2020-12-31T23:59:59Z", successfulCount: 0, negotiationFailureCount: 5, policyFailureCount: 6, generalFailureCount: 7)
            };

            StoreReports(reports);

            List<ReportLine> result = await _service.GetSummaryDetails("testReportDomain", 14);

            Assert.AreEqual(6, result.Count);

            Assert.AreEqual("microsoft.com", result[0].OrganizationName);
            Assert.AreEqual("2020-12-18", result[0].Date);
            Assert.AreEqual(0, result[0].FailedSessionCount);
            Assert.AreEqual(0, result[0].TotalFailureSessionCount);
            Assert.AreEqual(1, result[0].TotalSuccessfulSessionCount);
            Assert.AreEqual(null, result[0].ResultType);

            Assert.AreEqual("google.com", result[1].OrganizationName);
            Assert.AreEqual("2020-12-31", result[1].Date);
            Assert.AreEqual(4, result[1].FailedSessionCount);
            Assert.AreEqual(7, result[1].TotalFailureSessionCount);
            Assert.AreEqual(2, result[1].TotalSuccessfulSessionCount);
            Assert.AreEqual("DaneRequired", result[1].ResultType);

            Assert.AreEqual("google.com", result[2].OrganizationName);
            Assert.AreEqual("2020-12-31", result[2].Date);
            Assert.AreEqual(3, result[2].FailedSessionCount);
            Assert.AreEqual(7, result[2].TotalFailureSessionCount);
            Assert.AreEqual(2, result[2].TotalSuccessfulSessionCount);
            Assert.AreEqual("StartTlsNotSupported", result[2].ResultType);

            Assert.AreEqual("microsoft.com", result[3].OrganizationName);
            Assert.AreEqual("2020-12-31", result[3].Date);
            Assert.AreEqual(6, result[3].FailedSessionCount);
            Assert.AreEqual(18, result[3].TotalFailureSessionCount);
            Assert.AreEqual(0, result[3].TotalSuccessfulSessionCount);
            Assert.AreEqual("DaneRequired", result[3].ResultType);

            Assert.AreEqual("microsoft.com", result[4].OrganizationName);
            Assert.AreEqual("2020-12-31", result[4].Date);
            Assert.AreEqual(5, result[4].FailedSessionCount);
            Assert.AreEqual(18, result[4].TotalFailureSessionCount);
            Assert.AreEqual(0, result[4].TotalSuccessfulSessionCount);
            Assert.AreEqual("StartTlsNotSupported", result[4].ResultType);

            Assert.AreEqual("microsoft.com", result[5].OrganizationName);
            Assert.AreEqual("2020-12-31", result[5].Date);
            Assert.AreEqual(7, result[5].FailedSessionCount);
            Assert.AreEqual(18, result[5].TotalFailureSessionCount);
            Assert.AreEqual(0, result[5].TotalSuccessfulSessionCount);
            Assert.AreEqual("StsPolicyInvalid", result[5].ResultType);
        }

        [Test]
        public async Task GetSummaryDetailsReturnsMinimallyPopulatedReport()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            ReportInfo reportInfoWithRequiredPropertiesOnly = new ReportInfo(
                new JsonReport
                {
                    OrganizationName = "testOrganisationName",

                    DateRange = new DateRange
                    {
                        StartDatetime = DateTime.Parse("1990-01-01T00:00:00Z"),
                        EndDatetime = DateTime.Parse("2020-12-31T00:00:00Z"),
                    },

                    ReportId = "testReportId",

                    Policies = new List<PolicySummary>
                    {
                        new PolicySummary
                        {
                            Policy = new Policy
                            {
                                PolicyType = PolicyType.Sts,
                                PolicyDomain = "testPolicyDomain"
                            },
                            Summary = new Summary
                            {
                                TotalSuccessfulSessionCount = 1,
                                TotalFailureSessionCount = 2
                            },
                            FailureDetails = null
                        }
                    }
                }, "testVersion", "testSource", "testReportDomain", "testProvider");


            StoreReports(new[] { reportInfoWithRequiredPropertiesOnly });

            List<ReportLine> result = await _service.GetSummaryDetails("testReportDomain", 14);


            Assert.AreEqual(1, result.Count);

            Assert.AreEqual(null, result[0].AdditionalInformation);
            Assert.AreEqual("1990-01-01", result[0].Date);
            Assert.AreEqual(0, result[0].FailedSessionCount);
            Assert.AreEqual(null, result[0].FailureReasonCode);
            Assert.AreEqual(null, result[0].MxHost);
            Assert.AreEqual("testOrganisationName", result[0].OrganizationName);
            Assert.AreEqual("testPolicyDomain", result[0].PolicyDomain);
            Assert.AreEqual(null, result[0].PolicyString);
            Assert.AreEqual("Sts", result[0].PolicyType);
            Assert.AreEqual(null, result[0].ReceivingIp);
            Assert.AreEqual(null, result[0].ReceivingMxHelo);
            Assert.AreEqual(null, result[0].ReceivingMxHostname);
            Assert.AreEqual(null, result[0].ResultType);
            Assert.AreEqual(null, result[0].SendingMtaIp);
            Assert.AreEqual("testProvider", result[0].TlsReportSubmitter);
            Assert.AreEqual(2, result[0].TotalFailureSessionCount);
            Assert.AreEqual(1, result[0].TotalSuccessfulSessionCount);
        }

        [Test]
        public async Task GetSummaryDetailsReturnsFullyPopulatedReport()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            ReportInfo reportInfoWithRequiredPropertiesOnly = new ReportInfo(
                new JsonReport
                {
                    OrganizationName = "testOrganisationName",

                    DateRange = new DateRange
                    {
                        StartDatetime = DateTime.Parse("1990-01-01T00:00:00Z"),
                        EndDatetime = DateTime.Parse("2020-12-31T00:00:00Z"),
                    },

                    ReportId = "testReportId",

                    Policies = new List<PolicySummary>
                    {
                        new PolicySummary
                        {
                            Policy = new Policy
                            {
                                PolicyType = PolicyType.Sts,
                                PolicyDomain = "testPolicyDomain",
                                MxHost = new[] {"testMxHost1", "testMxHost2"},
                                PolicyString = new[] {"testPolicyString1", "testPolicyString2" }
                            },
                            Summary = new Summary
                            {
                                TotalSuccessfulSessionCount = 1,
                                TotalFailureSessionCount = 2
                            },
                            FailureDetails = new List<FailureDetail>
                            {
                                new FailureDetail
                                {
                                    AdditionalInformation = "testAdditionalInformation",
                                    FailedSessionCount = 3,
                                    FailureReasonCode = "testFailureReasonCode",
                                    ReceivingIp = "testReceivingIp",
                                    ReceivingMxHelo = "testReceivingMxHelo",
                                    ReceivingMxHostname = "testRecevingMxHostname",
                                    ResultType = ResultType.CertificateExpired,
                                    SendingMtaIp = "testSendingMtaIp"
                                }
                            }
                        }
                    }
                }, "testVersion", "testSource", "testReportDomain", "testProvider");


            StoreReports(new[] { reportInfoWithRequiredPropertiesOnly });

            List<ReportLine> result = await _service.GetSummaryDetails("testReportDomain", 14);

            Assert.AreEqual(1, result.Count);

            Assert.AreEqual("testAdditionalInformation", result[0].AdditionalInformation);
            Assert.AreEqual("1990-01-01", result[0].Date);
            Assert.AreEqual(3, result[0].FailedSessionCount);
            Assert.AreEqual("testFailureReasonCode", result[0].FailureReasonCode);
            Assert.AreEqual("testMxHost1, testMxHost2", result[0].MxHost);
            Assert.AreEqual("testOrganisationName", result[0].OrganizationName);
            Assert.AreEqual("testPolicyDomain", result[0].PolicyDomain);
            Assert.AreEqual("testPolicyString1, testPolicyString2", result[0].PolicyString);
            Assert.AreEqual("Sts", result[0].PolicyType);
            Assert.AreEqual("testReceivingIp", result[0].ReceivingIp);
            Assert.AreEqual("testReceivingMxHelo", result[0].ReceivingMxHelo);
            Assert.AreEqual("testRecevingMxHostname", result[0].ReceivingMxHostname);
            Assert.AreEqual("CertificateExpired", result[0].ResultType);
            Assert.AreEqual("testSendingMtaIp", result[0].SendingMtaIp);
            Assert.AreEqual("testProvider", result[0].TlsReportSubmitter);
            Assert.AreEqual(2, result[0].TotalFailureSessionCount);
            Assert.AreEqual(1, result[0].TotalSuccessfulSessionCount);
        }

        [Test]
        public async Task GetSummaryDetailsCalculatesFromMidnight()
        {
            A.CallTo(() => _clock.GetDateTimeUtc()).Returns(DateTime.Parse("2021-01-01T10:00:00Z"));

            List<ReportInfo> reports = new List<ReportInfo>
            {
                CreateReport("google.com", startDate: "2020-12-30T00:00:00Z", endDate:"2020-12-31T00:00:01Z", negotiationFailureCount: 1),
            };

            StoreReports(reports);

            List<ReportLine> result = await _service.GetSummaryDetails("testReportDomain", 1);

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

            List<ReportLine> result = await _service.GetSummaryDetails("testReportDomain", 1);

            Assert.AreEqual(0, result.Count);
        }
    }
}