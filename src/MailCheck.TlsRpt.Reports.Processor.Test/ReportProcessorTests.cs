using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using FakeItEasy;
using MailCheck.TlsRpt.Reports.Contracts;
using MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809;
using MailCheck.TlsRpt.Reports.Processor.Dao;
using MailCheck.TlsRpt.Reports.Processor.Domain;
using MailCheck.TlsRpt.Reports.Processor.Parser;
using MailCheck.TlsRpt.Reports.Processor.S3;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Processor.Test
{
    [TestFixture]
    public class ReportProcessorTests
    {
        private ReportProcessor _reportProcessor;
        private ILogger<ReportProcessor> _logger;
        private IS3Client _client;
        private IConfig _config;
        private IReportParser _parser;
        private ITlsRptDao _dao;
        private IS3SourceInfoFactory _s3SourceInfoFactory;
        private IEmailBodyParser _emailBodyParser;

        [SetUp]
        public void SetUp()
        {
            _logger = A.Fake<ILogger<ReportProcessor>>();
            _client = A.Fake<IS3Client>();
            _config = A.Fake<IConfig>();
            _parser = A.Fake<IReportParser>();
            _dao = A.Fake<ITlsRptDao>();
            _emailBodyParser = A.Fake<IEmailBodyParser>();
            _s3SourceInfoFactory = A.Fake<IS3SourceInfoFactory>();
            _reportProcessor = new ReportProcessor(_logger, _client, _config, _parser, _dao, _s3SourceInfoFactory, _emailBodyParser);
        }

        [Test]
        public async Task ProcessOrchestratesReportParsingAndSaves()
        {
            SQSEvent sqsEvent = new SQSEvent();
            string requestId = "testRequestId";
            S3SourceInfo s3SourceInfoFromFactory = new S3SourceInfo(null, null, 0, null, null);

            A.CallTo(() => _s3SourceInfoFactory.Create(sqsEvent, requestId)).Returns(new List<S3SourceInfo> { s3SourceInfoFromFactory });
            A.CallTo(() => _config.MaxS3ObjectSizeKilobytes).Returns(2);

            byte[] emailStream = new byte[1024];
            S3Object s3ObjectFromS3 = new S3Object(emailStream);
            A.CallTo(() => _client.GetS3Object(s3SourceInfoFromFactory)).Returns(s3ObjectFromS3);

            TlsRptEmail tlsRptEmailFromParser = new TlsRptEmail();
            A.CallTo(() => _emailBodyParser.Parse(emailStream)).Returns(tlsRptEmailFromParser);

            ReportInfo reportInfoFromParser = new ReportInfo(new JsonReport(), "", "", "", "");
            A.CallTo(() => _parser.Parse(tlsRptEmailFromParser, A<string>._)).Returns(reportInfoFromParser);

            await _reportProcessor.Process(sqsEvent, requestId);

            A.CallTo(() => _dao.Persist(reportInfoFromParser)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task ProcessOrchestratesReportParsingAndDoesNotSave()
        {
            SQSEvent sqsEvent = new SQSEvent();
            const string requestId = "testRequestId";
            S3SourceInfo s3SourceInfoFromFactory = new S3SourceInfo(null, null, 0, null, null);

            A.CallTo(() => _s3SourceInfoFactory.Create(sqsEvent, requestId)).Returns(new List<S3SourceInfo> { s3SourceInfoFromFactory });
            A.CallTo(() => _config.MaxS3ObjectSizeKilobytes).Returns(2);

            byte[] emailStream = new byte[1024];
            S3Object s3ObjectFromS3 = new S3Object(emailStream);
            A.CallTo(() => _client.GetS3Object(s3SourceInfoFromFactory)).Returns(s3ObjectFromS3);

            TlsRptEmail tlsRptEmailFromParser = new TlsRptEmail();
            A.CallTo(() => _emailBodyParser.Parse(emailStream)).Returns(tlsRptEmailFromParser);

            A.CallTo(() => _parser.Parse(tlsRptEmailFromParser, A<string>._)).Returns(new ReportInfo(null, "", "", "", ""));

            await _reportProcessor.Process(sqsEvent, requestId);

            A.CallTo(() => _parser.Parse(tlsRptEmailFromParser, A<string>._)).MustHaveHappened();
            A.CallTo(() => _dao.Persist(A<ReportInfo>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task ProcessIgnoresLargeObjects()
        {
            SQSEvent sqsEvent = new SQSEvent();
            const string requestId = "testRequestId";
            S3SourceInfo s3SourceInfoFromFactory = new S3SourceInfo(null, null, 0, null, null);

            A.CallTo(() => _s3SourceInfoFactory.Create(sqsEvent, requestId)).Returns(new List<S3SourceInfo> { s3SourceInfoFromFactory });
            A.CallTo(() => _config.MaxS3ObjectSizeKilobytes).Returns(0);

            byte[] emailStream = new byte[1024];
            S3Object s3ObjectFromS3 = new S3Object(emailStream);
            A.CallTo(() => _client.GetS3Object(s3SourceInfoFromFactory)).Returns(s3ObjectFromS3);


            await _reportProcessor.Process(sqsEvent, requestId);

            A.CallTo(() => _dao.Persist(A<ReportInfo>._)).MustNotHaveHappened();
        }
    }
}