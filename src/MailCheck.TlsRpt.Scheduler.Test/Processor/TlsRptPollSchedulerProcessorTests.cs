using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.Scheduler;
using MailCheck.TlsRpt.Scheduler.Config;
using MailCheck.TlsRpt.Scheduler.Dao;
using MailCheck.TlsRpt.Scheduler.Dao.Model;
using MailCheck.TlsRpt.Scheduler.Processor;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Scheduler.Test.Processor
{
    [TestFixture]
    public class TlsRptPollSchedulerProcessorTests
    {
        private TlsRptPollSchedulerProcessor _sut;
        private ITlsRptPeriodicSchedulerDao _dao;
        private IMessagePublisher _publisher;
        private ITlsRptPeriodicSchedulerConfig _config;
        private ILogger<TlsRptPollSchedulerProcessor> _log;

        [SetUp]
        public void SetUp()
        {
            _dao = A.Fake<ITlsRptPeriodicSchedulerDao>();
            _publisher = A.Fake<IMessagePublisher>();
            _config = A.Fake<ITlsRptPeriodicSchedulerConfig>();
            _log = A.Fake<ILogger<TlsRptPollSchedulerProcessor>>();

            _sut = new TlsRptPollSchedulerProcessor(_dao, _publisher, _config, _log);
        }

        [Test]
        public async Task ItShouldPublishAndUpdateThenContinueWhenThereAreExpiredRecords()
        {
            A.CallTo(() => _dao.GetExpiredTlsRptRecords())
                .Returns(CreateSchedulerStates("ncsc.gov.uk", "fco.gov.uk"));

            ProcessResult result = await _sut.Process();

            A.CallTo(() => _publisher.Publish(A<TlsRptRecordExpired>._, A<string>._))
                .MustHaveHappenedTwiceExactly();

            A.CallTo(() => _dao.UpdateLastChecked(A<List<TlsRptSchedulerState>>._))
                .MustHaveHappenedOnceExactly();

            Assert.AreEqual(ProcessResult.Continue, result);
        }

        [Test]
        public async Task ItShouldNotPublishOrUpdateThenStopWhenThereAreNoExpiredRecords()
        {
            A.CallTo(() => _dao.GetExpiredTlsRptRecords())
                .Returns(CreateSchedulerStates());

            ProcessResult result = await _sut.Process();

            A.CallTo(() => _publisher.Publish(A<TlsRptRecordExpired>._, A<string>._))
                .MustNotHaveHappened();

            A.CallTo(() => _dao.UpdateLastChecked(A<List<TlsRptSchedulerState>>._))
                .MustNotHaveHappened();

            Assert.AreEqual(ProcessResult.Stop, result);
        }


        private List<TlsRptSchedulerState> CreateSchedulerStates(params string[] args) =>
            args.Select(_ => new TlsRptSchedulerState(_)).ToList();
    }
}
