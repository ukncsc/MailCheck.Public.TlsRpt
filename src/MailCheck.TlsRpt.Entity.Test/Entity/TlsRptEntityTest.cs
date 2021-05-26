using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Common.Exception;
using MailCheck.TlsRpt.Contracts;
using MailCheck.TlsRpt.Contracts.Entity;
using MailCheck.TlsRpt.Contracts.Evaluator;
using MailCheck.TlsRpt.Contracts.Scheduler;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using MailCheck.TlsRpt.Entity.Config;
using MailCheck.TlsRpt.Entity.Dao;
using MailCheck.TlsRpt.Entity.Entity;
using MailCheck.TlsRpt.Entity.Entity.Notifiers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Message = MailCheck.TlsRpt.Contracts.SharedDomain.Message;

namespace MailCheck.TlsRpt.Entity.Test.Entity
{
    [TestFixture]
    public class TlsRptEntityTest
    {
        private const string Id = "abc.com";

        private ITlsRptEntityDao _tlsRptEntityDao;
        private ITlsRptEntityConfig _tlsRptEntityConfig;
        private ILogger<TlsRptEntity> _log;
        private IMessageDispatcher _dispatcher;
        private IChangeNotifiersComposite _changeNotifiersComposite;
        private TlsRptEntity _tlsRptEntity;

        [SetUp]
        public void SetUp()
        {
            _tlsRptEntityDao = A.Fake<ITlsRptEntityDao>();
            _tlsRptEntityConfig = A.Fake<ITlsRptEntityConfig>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _changeNotifiersComposite = A.Fake<IChangeNotifiersComposite>();
            _log = A.Fake<ILogger<TlsRptEntity>>();
            _tlsRptEntity = new TlsRptEntity(_tlsRptEntityDao, _tlsRptEntityConfig, _dispatcher, _changeNotifiersComposite, _log);
        }

        [Test]
        public async Task HandleDomainCreatedCreatesDomain()
        {
            A.CallTo(() => _tlsRptEntityDao.Get(Id)).Returns<TlsRptEntityState>(null);
            await _tlsRptEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now));

            A.CallTo(() => _tlsRptEntityDao.Save(A<TlsRptEntityState>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<TlsRptEntityCreated>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleDomainCreatedThrowsIfEntityAlreadyExistsForDomain()
        {
            A.CallTo(() => _tlsRptEntityDao.Get(Id)).Returns(new TlsRptEntityState(Id, 1, TlsRptState.PollPending, DateTime.UtcNow));
            await _tlsRptEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now));

            A.CallTo(() => _tlsRptEntityDao.Save(A<TlsRptEntityState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<TlsRptEntityCreated>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleTlsRptRecordExpiredRaiseTlsRptPollPending()
        {
            A.CallTo(() => _tlsRptEntityDao.Get(Id)).Returns(new TlsRptEntityState(Id, 2, TlsRptState.PollPending, DateTime.Now)
            {
                LastUpdated = DateTime.Now.AddDays(-1),
                TlsRptRecords = CreateTlsRptRecords(),
                TlsRptState = TlsRptState.Created
            });

            await _tlsRptEntity.Handle(new TlsRptRecordExpired(Id));

            A.CallTo(() => _tlsRptEntityDao.Save(A<TlsRptEntityState>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<TlsRptPollPending>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleTlsRptRecordsEvaluatedAndNewEvaluationUpdatesStateAndPublishes()
        {
            A.CallTo(() => _tlsRptEntityDao.Get(Id)).Returns(
                new TlsRptEntityState(Id, 2, TlsRptState.PollPending, DateTime.Now)
                {
                    LastUpdated = DateTime.Now.AddDays(-1),
                    TlsRptRecords = CreateTlsRptRecords()
                });

            TlsRptEntityState entityState = CreateTlsRptEntityState();

            entityState.Messages.Add(new Message(Guid.NewGuid(), MessageSources.TlsRptEvaluator, MessageType.error,
                "EvaluationError", string.Empty));
            entityState.TlsRptRecords.Records[0].Tags[0].Explanation = "Explanation";

            TlsRptRecordsEvaluated recordsEvaluated = new TlsRptRecordsEvaluated(Id, entityState.TlsRptRecords,
                new List<Message>
                {
                    new Message(Guid.NewGuid(), MessageSources.TlsRptEvaluator, MessageType.error, "EvaluationError",
                        string.Empty)
                }, DateTime.MinValue);

            await _tlsRptEntity.Handle(recordsEvaluated);

            A.CallTo(() => _dispatcher.Dispatch(A<TlsRptRecordEvaluationsChanged>.That.Matches(
                    _ => _.Messages[0].Text.Equals(entityState.Messages[0].Text) && _.Records.Records[0].Tags[0].Explanation.Equals(entityState.TlsRptRecords.Records[0].Tags[0].Explanation)), A<string>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _tlsRptEntityDao.Save(A<TlsRptEntityState>._)).MustHaveHappenedOnceExactly();
        }

        private static TlsRptEntityState CreateTlsRptEntityState(string domain = Id, string record = "v=TLSRPTv1;rua=mailto:tlsrpt@example.com")
        {
            TlsRptEntityState state =  new TlsRptEntityState(domain, 1, TlsRptState.Created, DateTime.UtcNow);
            state.TlsRptRecords = CreateTlsRptRecords(Id, record: record);
            return state;
        }

        private static TlsRptRecords CreateTlsRptRecords(string domain = Id,
            string record = "v=TLSRPTv1;rua=mailto:tlsrpt@example.com")
        {
            return new TlsRptRecords(domain, new List<TlsRptRecord>
            {
                new TlsRptRecord(domain, record.Split(";").ToList(), new List<Tag> {new UnknownTag("UnknownTag")})
            }, 100);
        }
    }
}
