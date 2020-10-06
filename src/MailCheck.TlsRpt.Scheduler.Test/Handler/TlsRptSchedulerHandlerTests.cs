using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.Entity;
using MailCheck.TlsRpt.Contracts.Scheduler;
using MailCheck.TlsRpt.Scheduler.Config;
using MailCheck.TlsRpt.Scheduler.Dao;
using MailCheck.TlsRpt.Scheduler.Dao.Model;
using MailCheck.TlsRpt.Scheduler.Handler;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Scheduler.Test.Handler
{
    [TestFixture]
    public class TlsRptSchedulerHandlerTests
    {
        private TlsRptSchedulerHandler _sut;
        private ITlsRptSchedulerDao _dao;
        private IMessageDispatcher _dispatcher;
        private ITlsRptSchedulerConfig _config;
        private ILogger<TlsRptSchedulerHandler> _log;

        [SetUp]
        public void SetUp()
        {
            _dao = A.Fake<ITlsRptSchedulerDao>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _config = A.Fake<ITlsRptSchedulerConfig>();
            _log = A.Fake<ILogger<TlsRptSchedulerHandler>>();

            _sut = new TlsRptSchedulerHandler(_dao, _dispatcher, _config, _log);
        }

        [Test]
        public async Task ItShouldSaveAndDispatchTheTlsRptStateIfItDoesntExist()
        {
            A.CallTo(() => _dao.Get(A<string>._)).Returns<TlsRptSchedulerState>(null);

            await _sut.Handle(new TlsRptEntityCreated("ncsc.gov.uk", 1));

            A.CallTo(() => _dao.Save(A<TlsRptSchedulerState>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _dispatcher.Dispatch(A<TlsRptRecordExpired>._, A<string>._))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task ItShouldNotSaveOrDispatchTheTlsRptStateIfItExists()
        {
            A.CallTo(() => _dao.Get(A<string>._)).Returns(new TlsRptSchedulerState("ncsc.gov.uk"));

            await _sut.Handle(new TlsRptEntityCreated("ncsc.gov.uk", 1));

            A.CallTo(() => _dao.Save(A<TlsRptSchedulerState>._)).MustNotHaveHappened();

            A.CallTo(() => _dispatcher.Dispatch(A<TlsRptRecordExpired>._, A<string>._))
                .MustNotHaveHappened();
        }
    }
}
