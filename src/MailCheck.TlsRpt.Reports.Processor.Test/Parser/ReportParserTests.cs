using System;
using System.Collections.Generic;
using System.Text;
using FakeItEasy;
using MailCheck.TlsRpt.Reports.Contracts;
using MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809;
using MailCheck.TlsRpt.Reports.Processor.Domain;
using MailCheck.TlsRpt.Reports.Processor.Parser;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Processor.Test.Parser
{
    public class ReportParserTests
    {
        private ReportParser _reportParser;
        private ILogger<ReportParser> _logger;
        private IAttachmentParser _attachmentParser;

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger<ReportParser>>();
            _attachmentParser = A.Fake<IAttachmentParser>();

            _reportParser = new ReportParser(_logger, _attachmentParser);
        }

        [Test]
        public void ParseThrowsIfNoAttachments()
        {
            TlsRptEmail tlsRptEmail = new TlsRptEmail { Attachments = new List<AttachmentInfo>() };

            ApplicationException result = Assert.Throws<ApplicationException>(() => _reportParser.Parse(tlsRptEmail, ""));

            Assert.AreEqual("Failed to parse: no attachment found where one was expected.", result.Message);
        }

        [Test]
        public void ParseThrowsIfMultipleAttachments()
        {
            TlsRptEmail tlsRptEmail = new TlsRptEmail
            {
                Attachments = new List<AttachmentInfo>
                {
                    new AttachmentInfo("testFilename1", null),
                    new AttachmentInfo("testFilename2", null)
                }
            };

            ApplicationException result = Assert.Throws<ApplicationException>(() => _reportParser.Parse(tlsRptEmail, ""));

            Assert.AreEqual($"Failed to parse: multiple attachments found where only one was expected. {Environment.NewLine} Attachment filenames: testFilename1, testFilename2", result.Message);
        }

        [Test]
        public void ParseParsesEmail()
        {
            AttachmentInfo attachmentInfo = new AttachmentInfo("testFilename1", null);
            TlsRptEmail tlsRptEmail = new TlsRptEmail
            {
                Attachments = new List<AttachmentInfo>
                {
                    attachmentInfo
                },
                TlsReportDomain = "testTlsReportDomain",
                TlsReportSubmitter = "testTlsReportSubmitter"
            };

            JsonReport jsonReportFromFactory = new JsonReport();
            A.CallTo(() => _attachmentParser.Parse(attachmentInfo)).Returns(jsonReportFromFactory);

            ReportInfo result = _reportParser.Parse(tlsRptEmail, "");

            Assert.AreEqual(jsonReportFromFactory, result.Report);
            Assert.AreEqual("testTlsReportDomain", result.TlsReportDomain);
            Assert.AreEqual("testTlsReportSubmitter", result.TlsReportSubmitter);
            Assert.AreEqual("RFC-8460_2018-09", result.Version);
        }

        [Test]
        public void ParseSetsUnknownVersion()
        {
            AttachmentInfo attachmentInfo = new AttachmentInfo("testFilename1", null);
            TlsRptEmail tlsRptEmail = new TlsRptEmail
            {
                Attachments = new List<AttachmentInfo>
                {
                    attachmentInfo
                },
                TlsReportDomain = "testTlsReportDomain",
                TlsReportSubmitter = "testTlsReportSubmitter"
            };

            JsonReport jsonReportFromFactory = null;
            A.CallTo(() => _attachmentParser.Parse(attachmentInfo)).Returns(null);

            ReportInfo result = _reportParser.Parse(tlsRptEmail, "");

            Assert.IsNull(jsonReportFromFactory);
            Assert.AreEqual("testTlsReportDomain", result.TlsReportDomain);
            Assert.AreEqual("testTlsReportSubmitter", result.TlsReportSubmitter);
            Assert.AreEqual("unknown", result.Version);
        }
    }
}
