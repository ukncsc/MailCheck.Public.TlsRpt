using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Contracts.External;
using MailCheck.TlsRpt.Reports.Contracts;
using MailCheck.TlsRpt.Reports.Entity.Config;
using MailCheck.TlsRpt.Reports.Entity.Dao;
using MailCheck.TlsRpt.Reports.Entity.Entity;
using MailCheck.TlsRpt.Reports.Entity.Entity.Notifiers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Entity.Test.Entity
{
    public class TlsRptReportsEntityTests
    {
        private const string Id = "abc.com";

        private ITlsRptReportsEntityDao _tlsRptReportsEntityDao;
        private IReportsDataDao _tlsRptReportsDataDao;
        private ILogger<TlsRptReportsEntity> _log;
        private IChangeNotifiersComposite _changeNotifiersComposite;
        private IEvaluator<DomainProvidersResults> _evaluator;
        private IClock _clock;
        private ITlsRptReportsEntityConfig _config;
        private IMessageDispatcher _dispatcher;
        private TlsRptReportsEntity _tlsRptReportsEntity;
        private IReportPeriodCalculator _reportPeriodCalculator;

        [SetUp]
        public void SetUp()
        {
            _tlsRptReportsEntityDao = A.Fake<ITlsRptReportsEntityDao>();
            _tlsRptReportsDataDao = A.Fake<IReportsDataDao>();
            _evaluator = A.Fake<IEvaluator<DomainProvidersResults>>();
            _changeNotifiersComposite = A.Fake<IChangeNotifiersComposite>();
            _log = A.Fake<ILogger<TlsRptReportsEntity>>();
            _clock = A.Fake<IClock>();
            _config = A.Fake<ITlsRptReportsEntityConfig>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _reportPeriodCalculator = A.Fake<IReportPeriodCalculator>();

            _tlsRptReportsEntity = new TlsRptReportsEntity(
                _tlsRptReportsEntityDao,
                _tlsRptReportsDataDao,
                _log,
                _changeNotifiersComposite,
                _evaluator,
                _clock,
                _config,
                _dispatcher,
                _reportPeriodCalculator);
        }

        [Test]
        public async Task HandleDomainCreatedCreatesScheduledReminder()
        {
            A.CallTo(() => _tlsRptReportsEntityDao.Read(Id)).Returns<TlsRptReportsEntityState>(null);
            await _tlsRptReportsEntity.Handle(new DomainCreated(Id, "Testuser", DateTime.Now));

            A.CallTo(() => _tlsRptReportsEntityDao.Create(A<TlsRptReportsEntityState>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<CreateScheduledReminder>.That.Matches(x => x.ResourceId == Id && x.Service == "TlsRptReports"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleDomainCreatedDoesNotCreateScheduledReminderIfEntityAlreadyExistsForDomain()
        {
            A.CallTo(() => _tlsRptReportsEntityDao.Read(Id)).Returns(new TlsRptReportsEntityState
            {
                Domain = Id,
                AdvisoryMessages = new List<ReportsAdvisoryMessage>(),
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                Version = 1
            });
            await _tlsRptReportsEntity.Handle(new DomainCreated(Id, "Testuser", DateTime.Now));

            A.CallTo(() => _tlsRptReportsEntityDao.Create(A<TlsRptReportsEntityState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<CreateScheduledReminder>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandlesTlsRptReportsScheduledReminderCorrectlyWhenEntityIsNull()
        {
            A.CallTo(() => _tlsRptReportsEntityDao.Read(Id)).Returns<TlsRptReportsEntityState>(null);
            await _tlsRptReportsEntity.Handle(new TlsRptReportsScheduledReminder("blah", Id));

            A.CallTo(() => _tlsRptReportsEntityDao.Update(A<TlsRptReportsEntityState>.That.Matches(x =>
                x.AdvisoryMessages.Count == 0 && x.Domain == Id && x.Version == 1))).MustHaveHappenedOnceExactly();
            A.CallTo(() => _changeNotifiersComposite.Handle(Id, A<List<ReportsAdvisoryMessage>>._, A<List<ReportsAdvisoryMessage>>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandlesTlsRptReportsScheduledReminderCorrectlyWhenEntityExists()
        {
            DateTime currentTime = _clock.GetDateTimeUtc();

            TlsRptReportsEntityState state = new TlsRptReportsEntityState
            {
                Domain = Id,
                Version = 1,
                AdvisoryMessages = new List<ReportsAdvisoryMessage>(),
                Created = currentTime,
                LastUpdated = currentTime
            };

            A.CallTo(() => _tlsRptReportsEntityDao.Read(Id)).Returns(state);
            await _tlsRptReportsEntity.Handle(new TlsRptReportsScheduledReminder("blah", Id));

            A.CallTo(() => _tlsRptReportsEntityDao.Update(A<TlsRptReportsEntityState>.That.Matches(x =>
                x.AdvisoryMessages.Count == 0 && x.Domain == Id && x.Version == 1))).MustHaveHappenedOnceExactly();
            A.CallTo(() => _changeNotifiersComposite.Handle(Id, A<List<ReportsAdvisoryMessage>>._, A<List<ReportsAdvisoryMessage>>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleDomainDeletedDispatchesDeleteScheduledReminder()
        {
            await _tlsRptReportsEntity.Handle(new DomainDeleted(Id));

            A.CallTo(() => _tlsRptReportsEntityDao.Delete(A<string>.That.Matches(x => x == Id))).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<DeleteScheduledReminder>.That.Matches(x => x.ResourceId == Id && x.Service == "TlsRptReports"), A<string>._)).MustHaveHappenedOnceExactly();
        }
    }
}
