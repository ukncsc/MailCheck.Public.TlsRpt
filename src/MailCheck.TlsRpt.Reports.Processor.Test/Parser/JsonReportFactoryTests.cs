using System;
using System.Collections.Generic;
using System.Text;
using FakeItEasy;
using MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809;
using MailCheck.TlsRpt.Reports.Processor.Domain;
using MailCheck.TlsRpt.Reports.Processor.Parser;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Processor.Test.Parser
{
    [TestFixture]
    public class JsonReportFactoryTests
    {
        private AttachmentParser _attachmentParser;
        private ILogger<AttachmentParser> _logger;

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger<AttachmentParser>>();
            _attachmentParser = new AttachmentParser(_logger);
        }

        [TestCase("sts", PolicyType.Sts, "certificate-expired", ResultType.CertificateExpired)]
        [TestCase("sts", PolicyType.Sts, "certificate-host-mismatch", ResultType.CertificateHostMismatch)]
        [TestCase("sts", PolicyType.Sts, "certificate-not-trusted", ResultType.CertificateNotTrusted)]
        [TestCase("sts", PolicyType.Sts, "dane-required", ResultType.DaneRequired)]
        [TestCase("sts", PolicyType.Sts, "dnssec-invalid", ResultType.DnssecInvalid)]
        [TestCase("sts", PolicyType.Sts, "starttls-not-supported", ResultType.StartTlsNotSupported)]
        [TestCase("sts", PolicyType.Sts, "sts-policy-fetch-error", ResultType.StsPolicyFetchError)]
        [TestCase("sts", PolicyType.Sts, "sts-policy-invalid", ResultType.StsPolicyInvalid)]
        [TestCase("sts", PolicyType.Sts, "sts-webpki-invalid", ResultType.StsWebpkiInvalid)]
        [TestCase("sts", PolicyType.Sts, "tlsa-invalid", ResultType.TlsaInvalid)]
        [TestCase("sts", PolicyType.Sts, "validation-failure", ResultType.ValidationFailure)]
        [TestCase("tlsa", PolicyType.Tlsa, "certificate-expired", ResultType.CertificateExpired)]
        [TestCase("tlsa", PolicyType.Tlsa, "certificate-host-mismatch", ResultType.CertificateHostMismatch)]
        [TestCase("tlsa", PolicyType.Tlsa, "certificate-not-trusted", ResultType.CertificateNotTrusted)]
        [TestCase("tlsa", PolicyType.Tlsa, "dane-required", ResultType.DaneRequired)]
        [TestCase("tlsa", PolicyType.Tlsa, "dnssec-invalid", ResultType.DnssecInvalid)]
        [TestCase("tlsa", PolicyType.Tlsa, "starttls-not-supported", ResultType.StartTlsNotSupported)]
        [TestCase("tlsa", PolicyType.Tlsa, "sts-policy-fetch-error", ResultType.StsPolicyFetchError)]
        [TestCase("tlsa", PolicyType.Tlsa, "sts-policy-invalid", ResultType.StsPolicyInvalid)]
        [TestCase("tlsa", PolicyType.Tlsa, "sts-webpki-invalid", ResultType.StsWebpkiInvalid)]
        [TestCase("tlsa", PolicyType.Tlsa, "tlsa-invalid", ResultType.TlsaInvalid)]
        [TestCase("tlsa", PolicyType.Tlsa, "validation-failure", ResultType.ValidationFailure)]
        [TestCase("no-policy-found", PolicyType.NoPolicyFound, "certificate-expired", ResultType.CertificateExpired)]
        [TestCase("no-policy-found", PolicyType.NoPolicyFound, "certificate-host-mismatch", ResultType.CertificateHostMismatch)]
        [TestCase("no-policy-found", PolicyType.NoPolicyFound, "certificate-not-trusted", ResultType.CertificateNotTrusted)]
        [TestCase("no-policy-found", PolicyType.NoPolicyFound, "dane-required", ResultType.DaneRequired)]
        [TestCase("no-policy-found", PolicyType.NoPolicyFound, "dnssec-invalid", ResultType.DnssecInvalid)]
        [TestCase("no-policy-found", PolicyType.NoPolicyFound, "starttls-not-supported", ResultType.StartTlsNotSupported)]
        [TestCase("no-policy-found", PolicyType.NoPolicyFound, "sts-policy-fetch-error", ResultType.StsPolicyFetchError)]
        [TestCase("no-policy-found", PolicyType.NoPolicyFound, "sts-policy-invalid", ResultType.StsPolicyInvalid)]
        [TestCase("no-policy-found", PolicyType.NoPolicyFound, "sts-webpki-invalid", ResultType.StsWebpkiInvalid)]
        [TestCase("no-policy-found", PolicyType.NoPolicyFound, "tlsa-invalid", ResultType.TlsaInvalid)]
        [TestCase("no-policy-found", PolicyType.NoPolicyFound, "validation-failure", ResultType.ValidationFailure)]
        public void CreateMapsKnownProperties(string rawPolicyType, PolicyType expectedPolicyType, string rawResultType, ResultType expectedResultType)
        {
            byte[] normalisedAttachment = Encoding.ASCII.GetBytes(GetRawReport(rawPolicyType, rawResultType));
            JsonReport report = _attachmentParser.Parse(new AttachmentInfo(null, normalisedAttachment));

            Assert.AreEqual("testContactInfo", report.ContactInfo);
            Assert.AreEqual(new DateTime(2000,01,01), report.DateRange.StartDatetime);
            Assert.AreEqual(new DateTime(2000,01,01,23,59,59), report.DateRange.EndDatetime);
            Assert.AreEqual("testOrganisationName", report.OrganizationName);
            Assert.AreEqual("testReportId", report.ReportId);
            Assert.AreEqual(1, report.Policies.Count);

            Policy policy = report.Policies[0].Policy;
            Assert.AreEqual(expectedPolicyType, policy.PolicyType);
            Assert.AreEqual("testPolicyString1", policy.PolicyString[0]);
            Assert.AreEqual("testPolicyString2", policy.PolicyString[1]);
            Assert.AreEqual("testPolicyDomain", policy.PolicyDomain);
            Assert.AreEqual("testMxHost1", policy.MxHost[0]);
            Assert.AreEqual("testMxHost2", policy.MxHost[1]);

            Summary summary = report.Policies[0].Summary;
            Assert.AreEqual(111, summary.TotalSuccessfulSessionCount);
            Assert.AreEqual(222, summary.TotalFailureSessionCount);

            List<FailureDetail> failureDetails = report.Policies[0].FailureDetails;
            Assert.AreEqual(1, failureDetails.Count);
            Assert.AreEqual(expectedResultType, failureDetails[0].ResultType);
            Assert.AreEqual("testSendingMtaIp", failureDetails[0].SendingMtaIp);
            Assert.AreEqual("testReceivingMxHostname", failureDetails[0].ReceivingMxHostname);
            Assert.AreEqual("testReceivingMxHelo", failureDetails[0].ReceivingMxHelo);
            Assert.AreEqual("testReceivingIp", failureDetails[0].ReceivingIp);
            Assert.AreEqual(333, failureDetails[0].FailedSessionCount);
            Assert.AreEqual("testAdditionalInformation", failureDetails[0].AdditionalInformation);
            Assert.AreEqual("testFailureReasonCode", failureDetails[0].FailureReasonCode);
        }

        [Test]
        public void CreateMapsExtraProperties()
        {
            string rawReport = GetRawReport();
            JObject jsonReport = JObject.Parse(rawReport);
            jsonReport["extra-root-property"] = "extra-root-value";
            jsonReport["date-range"]["extra-date-range-property"] = "extra-date-range-value";
            jsonReport["policies"][0]["extra-policy-root-property"] = "extra-policy-root-value";
            jsonReport["policies"][0]["policy"]["extra-policy-property"] = "extra-policy-value";
            jsonReport["policies"][0]["summary"]["extra-summary-property"] = "extra-summary-value";
            jsonReport["policies"][0]["failure-details"][0]["extra-failure-details-property"] = "extra-failure-details-value";

            byte[] normalisedAttachment = Encoding.ASCII.GetBytes(jsonReport.ToString());// GetRawReportWithExtraFields();

            JsonReport reportInfo = _attachmentParser.Parse(new AttachmentInfo(null, normalisedAttachment));

            Assert.AreEqual(reportInfo.AdditionalData["extra-root-property"].ToString(), "extra-root-value");
            Assert.AreEqual(reportInfo.DateRange.AdditionalData["extra-date-range-property"].ToString(), "extra-date-range-value");
            Assert.AreEqual(reportInfo.Policies[0].AdditionalData["extra-policy-root-property"].ToString(), "extra-policy-root-value");
            Assert.AreEqual(reportInfo.Policies[0].Policy.AdditionalData["extra-policy-property"].ToString(), "extra-policy-value");
            Assert.AreEqual(reportInfo.Policies[0].Summary.AdditionalData["extra-summary-property"].ToString(), "extra-summary-value");
            Assert.AreEqual(reportInfo.Policies[0].FailureDetails[0].AdditionalData["extra-failure-details-property"].ToString(), "extra-failure-details-value");
        }

        [Test]
        public void CreateDoesNotMapInvalidSchema()
        {
            byte[] normalisedAttachment = Encoding.ASCII.GetBytes(GetRawReport("something-unknown"));

            JsonReport reportInfo = _attachmentParser.Parse(new AttachmentInfo(null, normalisedAttachment));

            Assert.IsNull(reportInfo);
        }

        [Test]
        public void CreateDoesNotMapIfRequiredPropertyIsMissing()
        {
            string rawReport = GetRawReport();
            JObject jsonReport = JObject.Parse(rawReport);
            jsonReport["organization-name"] = null;

            byte[] normalisedAttachment = Encoding.ASCII.GetBytes(jsonReport.ToString());

            JsonReport reportInfo = _attachmentParser.Parse(new AttachmentInfo(null, normalisedAttachment));
            Assert.IsNull(reportInfo);
        }

        [Test]
        public void CreateMapsIfOptionalPropertiesAreMissing()
        {
            string rawReport = GetRawReport();
            JObject jsonReport = JObject.Parse(rawReport);
            jsonReport["policies"][0]["failure-details"][0]["receiving-mx-helo"] = null;
            jsonReport["policies"][0]["failure-details"][0]["additional-information"] = null;

            byte[] normalisedAttachment = Encoding.ASCII.GetBytes(jsonReport.ToString());

            JsonReport reportInfo = _attachmentParser.Parse(new AttachmentInfo(null, normalisedAttachment));
            Assert.IsNotNull(reportInfo);
        }

        private string GetRawReport(string rawPolicyType = "sts", string rawResultType = "certificate-expired")
        {
            string json = @"
            {
                'organization-name': 'testOrganisationName',
                'date-range': {
                    'start-datetime': '2000-01-01T00:00:00.000Z',
                    'end-datetime': '2000-01-01T23:59:59.000Z',
                },
                'contact-info': 'testContactInfo',
                'report-id': 'testReportId',
                'policies': [{
                    'policy': {
                        'policy-type': '" + rawPolicyType + @"',
                        'policy-string': ['testPolicyString1', 'testPolicyString2'],
                        'policy-domain': 'testPolicyDomain',
                        'mx-host': ['testMxHost1', 'testMxHost2']
                        },
                    'summary': {
                        'total-successful-session-count': 111,
                        'total-failure-session-count': 222
                        },
                    'failure-details': [{
                        'result-type': '" + rawResultType + @"',
                        'sending-mta-ip': 'testSendingMtaIp',
                        'receiving-mx-hostname': 'testReceivingMxHostname',
                        'receiving-mx-helo': 'testReceivingMxHelo',
                        'receiving-ip': 'testReceivingIp',
                        'failed-session-count': 333,
                        'additional-information': 'testAdditionalInformation',
                        'failure-reason-code': 'testFailureReasonCode'
                        }]
                    }]
             }";

            return json;
        }
    }
}
