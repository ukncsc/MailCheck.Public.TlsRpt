using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.TlsRpt.Poller.Config;
using MailCheck.TlsRpt.Poller.Dns;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Domain.Errors.Parser;
using MailCheck.TlsRpt.Poller.Exceptions;
using MailCheck.TlsRpt.Poller.Parsing;
using MailCheck.TlsRpt.Poller.Rules;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Poller.Test
{
    [TestFixture]
    public class TlsRptProcessorTests
    {
        private IDnsClient _dnsClient;
        private ITlsRptRecordsParser _tlsRptRecordsParser;
        private ITlsRptRecordsEvaluator _evaluator;
        private ITlsRptProcessor _tlsRptProcessor;
        private ITlsRptPollerConfig _config;
        private ILogger<TlsRptProcessor> _log;

        private const string ExampleTlsRptRecord =
            "v=TLSRPTv1;rua=mailto:tlsrpt@example.com";

        [SetUp]
        public void SetUp()
        {
            _dnsClient = A.Fake<IDnsClient>();
            _tlsRptRecordsParser = A.Fake<ITlsRptRecordsParser>();
            _config = A.Fake<ITlsRptPollerConfig>();
            _evaluator = A.Fake<ITlsRptRecordsEvaluator>();
            _log = A.Fake<ILogger<TlsRptProcessor>>();

            _tlsRptProcessor = new TlsRptProcessor(_dnsClient, _tlsRptRecordsParser, _evaluator, _config, _log);
        }


        [Test]
        public async Task TlsRptRecordsReturnedWhenAllGood()
        {
            string domain = "abc.com";

            TlsRptRecordInfo tlsRptRecordInfo =
                new TlsRptRecordInfo(domain, new List<string> {ExampleTlsRptRecord});

            TlsRptRecordInfos tlsRptRecordInfos =
                new TlsRptRecordInfos(domain, new List<TlsRptRecordInfo> {tlsRptRecordInfo}, 1);

            A.CallTo(() => _dnsClient.GetTlsRptRecords(domain))
                .Returns(Task.FromResult(tlsRptRecordInfos));

            TlsRptRecords tlsRptRecords = new TlsRptRecords(domain, new List<TlsRptRecord>(), 1);

            A.CallTo(() => _tlsRptRecordsParser.Parse(tlsRptRecordInfos))
                .Returns(new EvaluationResult<TlsRptRecords>(new TlsRptRecords(domain, new List<TlsRptRecord>(), 1)));

            A.CallTo(() => _evaluator.Evaluate(A<TlsRptRecords>._))
                .Returns(Task.FromResult(new EvaluationResult<TlsRptRecords>(tlsRptRecords)));
            TlsRptPollResult result = await _tlsRptProcessor.Process(domain);

            Assert.That(result.TlsRptRecords, Is.SameAs(tlsRptRecords));
        }



        [Test]
        public async Task TlsRptExceptionNotThrownWhenAllowNullResultsSetAndEmptyResult()
        {
            string domain = "abc.com";

            TlsRptRecordInfo tlsRptRecordInfo =
                new TlsRptRecordInfo(domain, new List<string>());

            TlsRptRecordInfos tlsRptRecordInfos =
                new TlsRptRecordInfos(domain, new List<TlsRptRecordInfo> {tlsRptRecordInfo}, 1);

            A.CallTo(() => _dnsClient.GetTlsRptRecords(domain))
                .Returns(Task.FromResult(tlsRptRecordInfos));

            A.CallTo(() => _tlsRptRecordsParser.Parse(tlsRptRecordInfos))
                .Returns(new EvaluationResult<TlsRptRecords>(new TlsRptRecords(domain, new List<TlsRptRecord>(), 1)));

            TlsRptPollResult result = await _tlsRptProcessor.Process(domain);

            Assert.AreEqual(0, result.TlsRptRecords.Records.Count);
        }

        [Test]
        public async Task ErrorResultReturnedWhenDnsErrorRetrievingTlsRptRecordTest()
        {
            string domain = "abc.com";
            Guid errorId = Guid.NewGuid();
            
            TlsRptRecordInfos tlsRptRecordInfos = new TlsRptRecordInfos(domain,
                new Error(errorId, "mailcheck.tlsrpt.testName", ErrorType.Error, "error", ""), 1, "nameserver", "auditTrail");

            A.CallTo(() => _dnsClient.GetTlsRptRecords(domain))
                .Returns(Task.FromResult(tlsRptRecordInfos));

            A.CallTo(() => _tlsRptRecordsParser.Parse(tlsRptRecordInfos))
                .Returns(new EvaluationResult<TlsRptRecords>(new TlsRptRecords(domain, new List<TlsRptRecord>(), 1)));

            TlsRptPollResult result = await _tlsRptProcessor.Process(domain);

            Assert.AreEqual(1, result.Errors.Count);
        }
    }
}