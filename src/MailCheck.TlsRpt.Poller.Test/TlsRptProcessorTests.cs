using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.TlsRpt.Poller.Config;
using MailCheck.TlsRpt.Poller.Dns;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Exceptions;
using MailCheck.TlsRpt.Poller.Parsing;
using MailCheck.TlsRpt.Poller.Rules;
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

        [SetUp]
        public void SetUp()
        {
            _dnsClient = A.Fake<IDnsClient>();
            _tlsRptRecordsParser = A.Fake<ITlsRptRecordsParser>();
            _config = A.Fake<ITlsRptPollerConfig>();
            _evaluator = A.Fake<ITlsRptRecordsEvaluator>();
            _tlsRptProcessor = new TlsRptProcessor(_dnsClient, _tlsRptRecordsParser, _evaluator, _config);
        }

        [Test]
        public async Task TlsRptExceptionThrownWhenAllowNullResultsNotSetAndEmptyResult()
        {
            string domain = "abc.com";

            A.CallTo(() => _config.AllowNullResults).Returns(false);

            Assert.Throws<TlsRptPollerException>(() => _tlsRptProcessor.Process(domain).GetAwaiter().GetResult());
        }

        [Test]
        public async Task TlsRptExceptionNotThrownWhenAllowNullResultsSetAndEmptyResult()
        {
            string domain = "abc.com";

            A.CallTo(() => _config.AllowNullResults).Returns(true);

            A.CallTo(() => _tlsRptRecordsParser.Parse(A<TlsRptRecordInfos>._)).Returns(
                new EvaluationResult<TlsRptRecords>(new TlsRptRecords(domain, new List<TlsRptRecord>(), 0)));

            A.CallTo(() => _evaluator.Evaluate(A<TlsRptRecords>._)).Returns(
                new EvaluationResult<TlsRptRecords>(new TlsRptRecords(domain, new List<TlsRptRecord>(), 0)));

            TlsRptPollResult result = await _tlsRptProcessor.Process(domain);

            Assert.AreEqual(0, result.TlsRptRecords.Records.Count);
        }
    }
}
