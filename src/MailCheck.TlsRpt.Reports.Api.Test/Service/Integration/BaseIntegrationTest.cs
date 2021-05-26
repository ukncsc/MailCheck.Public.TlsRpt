using System;
using System.Collections.Generic;
using MailCheck.TlsRpt.Reports.Contracts;
using MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809;
using Mongo2Go;
using MongoDB.Driver;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Api.Test.Service.Integration
{
    [TestFixture(Category = "Integration")]
    public class BaseIntegrationTest
    {
        internal static MongoDbRunner Runner;
        private MongoClient _client;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Runner = MongoDbRunner.Start();
            _client = new MongoClient(Runner.ConnectionString);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Runner.Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            IMongoDatabase database = _client.GetDatabase("test");
            IMongoCollection<ReportInfo> collection = database.GetCollection<ReportInfo>("reports");
            collection.DeleteMany(Builders<ReportInfo>.Filter.Empty);
        }

        protected void StoreReports(IEnumerable<ReportInfo> reportInfos)
        {
            IMongoDatabase database = _client.GetDatabase("test");
            IMongoCollection<ReportInfo> collection = database.GetCollection<ReportInfo>("reports");
            collection.InsertMany(reportInfos);
        }

        protected ReportInfo CreateReport(string provider, string startDate, string endDate, int successfulCount = 0, int negotiationFailureCount = 0, int policyFailureCount = 0, int generalFailureCount = 0)
        {
            List<FailureDetail> failureDetails = null;
            if (negotiationFailureCount > 0)
            {
                failureDetails = new List<FailureDetail>();
                failureDetails.Add(new FailureDetail { ResultType = ResultType.StartTlsNotSupported, FailedSessionCount = negotiationFailureCount });
            }
            if (policyFailureCount > 0)
            {
                failureDetails = failureDetails ?? new List<FailureDetail>();
                failureDetails.Add(new FailureDetail { ResultType = ResultType.DaneRequired, FailedSessionCount = policyFailureCount });
            }
            if (generalFailureCount > 0)
            {
                failureDetails = failureDetails ?? new List<FailureDetail>();
                failureDetails.Add(new FailureDetail { ResultType = ResultType.StsPolicyInvalid, FailedSessionCount = generalFailureCount });
            }

            ReportInfo reportInfoRequiredFieldsPopulated = new ReportInfo(
                new JsonReport
                {
                    OrganizationName = provider,

                    DateRange = new DateRange
                    {
                        StartDatetime = DateTime.Parse(startDate),
                        EndDatetime = DateTime.Parse(endDate),
                    },

                    ReportId = "testRepordId",

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
                                TotalSuccessfulSessionCount = successfulCount,
                                TotalFailureSessionCount =
                                    negotiationFailureCount + policyFailureCount + generalFailureCount
                            },
                            FailureDetails = failureDetails
                        }
                    }
                }, "testVersion", "testSource", "testReportDomain", provider);

            return reportInfoRequiredFieldsPopulated;
        }
    }
}