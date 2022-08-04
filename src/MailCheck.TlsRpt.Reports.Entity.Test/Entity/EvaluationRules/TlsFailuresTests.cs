using System;
using System.Collections.Generic;
using FakeItEasy;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Reports.Entity.Dao;
using MailCheck.TlsRpt.Reports.Entity.Entity.EvaluationRules;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using MailCheck.TlsRpt.Reports.Entity.Entity;
using MailCheck.TlsRpt.Reports.Contracts;

namespace MailCheck.TlsRpt.Reports.Entity.Test.Entity.EvaluationRules
{
    [TestFixture]
    public class TlsFailuresTests
    {
        private static readonly Guid TwoDaysSuccess = new Guid("81daf9c9-ee69-4220-a9af-4262fcd089cc");
        private static readonly Guid TwoDaysInfo = new Guid("9c2fd5e9-cef9-465b-945f-7aaed7416d62");
        private static readonly Guid TwoDaysWarning = new Guid("907af89b-dd38-4d73-9085-c9de21b30182");
        private static readonly Guid TwoDaysError = new Guid("39ad1004-e37e-45d5-9f27-b388b5fbeaf5");
        private static readonly Guid FourteenDaysSuccess = new Guid("f25e812d-99b7-4272-bd72-129a6889fd27");
        private static readonly Guid FourteenDaysInfo = new Guid("ecf33ec5-28bb-419a-8dd6-13c86ee89315");
        private static readonly Guid FourteenDaysWarning = new Guid("a29d3cc2-f64e-493d-8680-fb88ce2da89d");
        private static readonly Guid FourteenDaysError = new Guid("ac7fb401-c6fc-4e11-a6d8-3b65a18b11a7");
        private static readonly Guid NinetyDaysSuccess = new Guid("358b0f8f-22fe-441b-84a8-555167c991ab");
        private static readonly Guid NinetyDaysInfo = new Guid("d3c985c4-92e2-457d-94a1-88c4259d8665");
        private static readonly Guid NinetyDaysWarning = new Guid("4db39e11-b80d-4c6e-b822-e4f919610eb4");
        private static readonly Guid NinetyDaysError = new Guid("1225ebd3-e308-4417-b490-8a723885ca78");

        private readonly string Domain = "test.gov.uk";

        private ILogger<TlsFailures> _logger;
        private TlsFailures _tlsFailures;

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger<TlsFailures>>();

            _tlsFailures = new TlsFailures(_logger);
        }

        [TestCaseSource(nameof(ExerciseTlsFailuresTestPermutations))]
        public async System.Threading.Tasks.Task ExerciseTlsFailuresTestAsync(TlsFailuresTestCase testCase)
        {
            Dictionary<int, List<ProviderFailure>> providerFailures = new Dictionary<int, List<ProviderFailure>>
            {
                { 2, testCase.TwoDaysFailures },
                { 14, testCase.FourteenDaysFailures },
                { 90, testCase.NinetyDaysFailures }
            };

            Dictionary<int, Tuple<DateTime, DateTime>> periods = new Dictionary<int, Tuple<DateTime, DateTime>>
            {
                [2] = Tuple.Create(
                    new DateTime(2021, 5, 14, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2021, 5, 16, 0, 0, 0, DateTimeKind.Utc)
                ),
                [14] = Tuple.Create(
                    new DateTime(2021, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2021, 5, 15, 0, 0, 0, DateTimeKind.Utc)
                ),
                [90] = Tuple.Create(
                    new DateTime(2021, 2, 15, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2021, 5, 16, 0, 0, 0, DateTimeKind.Utc)
                ),
            };

            DomainProvidersResults domainProvidersResults = new DomainProvidersResults
            {
                Periods = periods,
                Domain = Domain,
                ProviderTotals = testCase.ProviderTotals,
                ProviderFailures = providerFailures
            };

            List<AdvisoryMessage> result = await _tlsFailures.Evaluate(domainProvidersResults);

            Assert.AreEqual(testCase.ExpectedAdvisories.Count, result.Count);

            for (int i = 0; i < result.Count; i++)
            {
                ReportsAdvisoryMessage actualAdvisory = (ReportsAdvisoryMessage)result[i];
                ReportsAdvisoryMessage expectedAdvisory = (ReportsAdvisoryMessage)testCase.ExpectedAdvisories[i];

                Assert.AreEqual(expectedAdvisory.Id, actualAdvisory.Id);
                Assert.AreEqual(expectedAdvisory.MessageType, actualAdvisory.MessageType);
                Assert.AreEqual(expectedAdvisory.Text, actualAdvisory.Text);
                Assert.AreEqual(expectedAdvisory.MarkDown, actualAdvisory.MarkDown);
                Assert.AreEqual(expectedAdvisory.Period, actualAdvisory.Period);
            }
        }

        private static IEnumerable<TlsFailuresTestCase> ExerciseTlsFailuresTestPermutations()
        {
            ProviderTotals successfulProvider = new ProviderTotals
            {
                Provider = "google.test1.com.",
                TotalFailureSessionCount = 0,
                TotalSuccessfulSessionCount = 5,
                ReportEndDate = new DateTime(2021, 5, 14, 23, 59, 59, DateTimeKind.Utc),
            };

            ProviderTotals successfulProvider2 = new ProviderTotals
            {
                Provider = "google.test12.com.",
                TotalFailureSessionCount = 0,
                TotalSuccessfulSessionCount = 7,
                ReportEndDate = new DateTime(2021, 5, 11, 23, 59, 59, DateTimeKind.Utc),
            };

            ProviderTotals successfulProvider3 = new ProviderTotals
            {
                Provider = "google.test12.com.",
                TotalFailureSessionCount = 0,
                TotalSuccessfulSessionCount = 13,
                ReportEndDate = new DateTime(2021, 4, 11, 23, 59, 59, DateTimeKind.Utc),
            };

            ProviderTotals failedProvider1 = new ProviderTotals
            {
                Provider = "google.test12.com.",
                TotalFailureSessionCount = 5,
                TotalSuccessfulSessionCount = 5,
                ReportEndDate = new DateTime(2021, 5, 14, 23, 59, 59, DateTimeKind.Utc),
            };

            ProviderTotals failedProvider2 = new ProviderTotals
            {
                Provider = "google.test123.com.",
                TotalFailureSessionCount = 10,
                TotalSuccessfulSessionCount = 5,
                ReportEndDate = new DateTime(2021, 5, 14, 23, 59, 59, DateTimeKind.Utc),
            };

            ProviderTotals failedProvider3 = new ProviderTotals
            {
                Provider = "google.test12.com.",
                TotalFailureSessionCount = 10,
                TotalSuccessfulSessionCount = 5,
                ReportEndDate = new DateTime(2021, 3, 14, 23, 59, 59, DateTimeKind.Utc),
            };

            ProviderFailure providerFailure1 = new ProviderFailure
            {
                Provider = "google.test12.com.",
                Percent = 50,
                ResultType = ResultType.CertificateExpired
            };

            ProviderFailure providerFailure2 = new ProviderFailure
            {
                Provider = "google.test123.com.",
                Percent = 66.66,
                ResultType = ResultType.DaneRequired
            };

            ProviderFailure providerFailure3 = new ProviderFailure
            {
                Provider = "google.test12.com.",
                Percent = 12,
                ResultType = ResultType.CertificateHostMismatch
            };

            TlsFailuresTestCase test0 = new TlsFailuresTestCase
            {
                ProviderTotals = new List<ProviderTotals>
                {
                    null, null, null
                },
                TwoDaysFailures = new List<ProviderFailure>
                {
                    null
                },
                FourteenDaysFailures = new List<ProviderFailure>
                {
                    null
                },
                NinetyDaysFailures = new List<ProviderFailure>
                {
                    null
                },
                ExpectedAdvisories = new List<AdvisoryMessage>(),
                Description = "null provider totals and provider failures should return no advisories"
            };

            TlsFailuresTestCase test1 = new TlsFailuresTestCase
            {
                ProviderTotals = new List<ProviderTotals>
                {
                    successfulProvider, successfulProvider2
                },
                TwoDaysFailures = new List<ProviderFailure>(),
                FourteenDaysFailures = new List<ProviderFailure>(),
                NinetyDaysFailures = new List<ProviderFailure>(),
                ExpectedAdvisories = new List<AdvisoryMessage>
                {
                    new ReportsAdvisoryMessage(
                        NinetyDaysSuccess, 
                        "mailcheck.tlsrpt.testName", 
                        MessageType.success, 
                        "In the last 90 days, no TLS failures were reported.", 
                        "- No failures were reported\n\n\nReports for the period 15 Feb 2021 to 15 May 2021", 
                        "90-days")
                },
                Description = "No Tls Failures should produce one success advisory for 90 days"
            };

            TlsFailuresTestCase test2 = new TlsFailuresTestCase
            {
                ProviderTotals = new List<ProviderTotals>
                {
                    successfulProvider, successfulProvider2, successfulProvider3, failedProvider1
                },
                TwoDaysFailures = new List<ProviderFailure>
                {
                    providerFailure1
                },
                FourteenDaysFailures = new List<ProviderFailure>
                {
                    providerFailure1
                },
                NinetyDaysFailures = new List<ProviderFailure>
                {
                    providerFailure1
                },
                ExpectedAdvisories = new List<AdvisoryMessage>
                {
                    new ReportsAdvisoryMessage(
                        TwoDaysWarning,
                        "mailcheck.tlsrpt.testName",
                        MessageType.warning,
                        "In the last 2 days, 1 email providers reported TLS failures accounting for 33.33% of all inbound sessions",
                        "- google.test12.com. reports that:\n"+
                        "    - 50% of their TLS sessions failed because the certificate has expired\n"+
                        "\n"+
                        "Reports for the period 14 May 2021 to 15 May 2021",
                        "2-days"),
                    new ReportsAdvisoryMessage(
                        FourteenDaysWarning,
                        "mailcheck.tlsrpt.testName",
                        MessageType.warning,
                        "In the last 14 days, 1 email providers reported TLS failures accounting for 22.73% of all inbound sessions",
                        "- google.test12.com. reports that:\n"+
                        "    - 50% of their TLS sessions failed because the certificate has expired\n"+
                        "\n"+
                        "Reports for the period 1 May 2021 to 14 May 2021",
                        "14-days"),
                    new ReportsAdvisoryMessage(
                        NinetyDaysWarning,
                        "mailcheck.tlsrpt.testName",
                        MessageType.warning,
                        "In the last 90 days, 1 email providers reported TLS failures accounting for 14.29% of all inbound sessions",
                        "- google.test12.com. reports that:\n"+
                        "    - 50% of their TLS sessions failed because the certificate has expired\n"+
                        "\n"+
                        "Reports for the period 15 Feb 2021 to 15 May 2021",
                        "90-days")
                },
                Description = "TLS Failure in last 2 days should produce 3 advisories"
            };

            TlsFailuresTestCase test3 = new TlsFailuresTestCase
            {
                ProviderTotals = new List<ProviderTotals>
                {
                    successfulProvider, failedProvider1, failedProvider2
                },
                TwoDaysFailures = new List<ProviderFailure>
                {
                    providerFailure1
                },
                FourteenDaysFailures = new List<ProviderFailure>
                {
                    providerFailure1
                },
                NinetyDaysFailures = new List<ProviderFailure>
                {
                    providerFailure1,
                    providerFailure2
                },
                ExpectedAdvisories = new List<AdvisoryMessage>
                {
                    new ReportsAdvisoryMessage(
                        TwoDaysError,
                        "mailcheck.tlsrpt.testName",
                        MessageType.error,
                        "In the last 2 days, 2 email providers reported TLS failures accounting for 50% of all inbound sessions",
                        "- google.test12.com. reports that:\n"+
                        "    - 50% of their TLS sessions failed because the certificate has expired\n"+
                        "\n"+
                        "Reports for the period 14 May 2021 to 15 May 2021",
                        "2-days"),
                    new ReportsAdvisoryMessage(
                        FourteenDaysError,
                        "mailcheck.tlsrpt.testName",
                        MessageType.error,
                        "In the last 14 days, 2 email providers reported TLS failures accounting for 50% of all inbound sessions",
                        "- google.test12.com. reports that:\n"+
                        "    - 50% of their TLS sessions failed because the certificate has expired\n"+
                        "\n"+
                        "Reports for the period 1 May 2021 to 14 May 2021",
                        "14-days"),
                    new ReportsAdvisoryMessage(
                        NinetyDaysError,
                        "mailcheck.tlsrpt.testName",
                        MessageType.error,
                        "In the last 90 days, 2 email providers reported TLS failures accounting for 50% of all inbound sessions",
                        "- google.test12.com. reports that:\n"+
                        "    - 50% of their TLS sessions failed because the certificate has expired\n"+
                        "- google.test123.com. reports that:\n"+
                        "    - 66.66% of their TLS sessions failed because a DANE failure occurred\n"+
                        "\n"+
                        "Reports for the period 15 Feb 2021 to 15 May 2021",
                        "90-days")
                },
                Description = "Larger Failures only in last 90 days should produce 3 advisories with error 90 days advisory"
            };

            TlsFailuresTestCase test4 = new TlsFailuresTestCase
            {
                ProviderTotals = new List<ProviderTotals>
                {
                    successfulProvider, failedProvider3
                },
                TwoDaysFailures = new List<ProviderFailure>
                {
                },
                FourteenDaysFailures = new List<ProviderFailure>
                {
                },
                NinetyDaysFailures = new List<ProviderFailure>
                {
                    providerFailure1
                },
                ExpectedAdvisories = new List<AdvisoryMessage>
                {
                    new ReportsAdvisoryMessage(
                        FourteenDaysSuccess,
                        "mailcheck.tlsrpt.testName",
                        MessageType.success,
                        "In the last 14 days, no TLS failures were reported.",
                        "- No failures were reported\n\n\nReports for the period 1 May 2021 to 14 May 2021",
                        "14-days"),
                    new ReportsAdvisoryMessage(
                        NinetyDaysError,
                        "mailcheck.tlsrpt.testName",
                        MessageType.error,
                        "In the last 90 days, 1 email providers reported TLS failures accounting for 50% of all inbound sessions",
                        "- google.test12.com. reports that:\n"+
                        "    - 50% of their TLS sessions failed because the certificate has expired\n"+
                        "\n"+
                        "Reports for the period 15 Feb 2021 to 15 May 2021",
                        "90-days")
                },
                Description = "Periods with no failures should be collapsed"
            };

            TlsFailuresTestCase test5 = new TlsFailuresTestCase
            {
                ProviderTotals = new List<ProviderTotals>
                {
                    successfulProvider, failedProvider3
                },
                TwoDaysFailures = new List<ProviderFailure>
                {
                },
                FourteenDaysFailures = new List<ProviderFailure>
                {
                },
                NinetyDaysFailures = new List<ProviderFailure>
                {
                    providerFailure1, providerFailure3
                },
                ExpectedAdvisories = new List<AdvisoryMessage>
                {
                    new ReportsAdvisoryMessage(
                        FourteenDaysSuccess,
                        "mailcheck.tlsrpt.testName",
                        MessageType.success,
                        "In the last 14 days, no TLS failures were reported.",
                        "- No failures were reported\n\n\nReports for the period 1 May 2021 to 14 May 2021",
                        "14-days"),
                    new ReportsAdvisoryMessage(
                        NinetyDaysError,
                        "mailcheck.tlsrpt.testName",
                        MessageType.error,
                        "In the last 90 days, 1 email providers reported TLS failures accounting for 50% of all inbound sessions",
                        "- google.test12.com. reports that:\n"+
                        "    - 50% of their TLS sessions failed because the certificate has expired\n"+
                        "    - 12% of their TLS sessions failed because the certificate presented did not adhere to the constraints specified in the MTA-STS or DANE policy\n"+
                        "\n"+
                        "Reports for the period 15 Feb 2021 to 15 May 2021",
                        "90-days")
                },
                Description = "Different failures for same provider should be grouped"
            };

            yield return test0;
            yield return test1;
            yield return test2;
            yield return test3;
            yield return test4;
            yield return test5;
        }


        public class TlsFailuresTestCase
        {
            public List<ProviderTotals> ProviderTotals { get; set; }
            public List<ProviderFailure> TwoDaysFailures { get; set; }
            public List<ProviderFailure> FourteenDaysFailures { get; set; }
            public List<ProviderFailure> NinetyDaysFailures { get; set; }
            public List<AdvisoryMessage> ExpectedAdvisories { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return Description;
            }
        }
    }
}