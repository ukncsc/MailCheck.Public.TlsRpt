using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.External;
using MailCheck.TlsRpt.Contracts.Poller;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using MailCheck.TlsRpt.EntityHistory.Config;
using MailCheck.TlsRpt.EntityHistory.Dao;
using MailCheck.TlsRpt.EntityHistory.Entity;
using MailCheck.TlsRpt.EntityHistory.Service;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.TlsRpt.EntityHistory.Test.Entity
{
    [TestFixture]
    public class TlsRptHistoryEntityTest
    {
        private const string Id = "abc.com";

        private ITlsRptEntityHistoryDao _entityDao;
        private ILogger<TlsRptEntityHistory> _log;
        private TlsRptEntityHistory _historyEntity;
        private ITlsRptEntityHistoryConfig _tlsRptEntityHistoryConfig;
        private IMessageDispatcher _dispatcher;
        private ITlsRptRuaService _tlsRptRuaService;

        [SetUp]
        public void SetUp()
        {
            _entityDao = A.Fake<ITlsRptEntityHistoryDao>();
            _tlsRptEntityHistoryConfig = A.Fake<ITlsRptEntityHistoryConfig>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _log = A.Fake<ILogger<TlsRptEntityHistory>>();
            _tlsRptRuaService = A.Fake<ITlsRptRuaService>();
            _historyEntity = new TlsRptEntityHistory(_dispatcher,_log, _entityDao, _tlsRptEntityHistoryConfig, _tlsRptRuaService);
        }

        [Test]
        public async Task HandleDomainCreatedCreatesDomain()
        {
            A.CallTo(() => _entityDao.Get(Id)).Returns<TlsRptEntityHistoryState>(null);
            await _historyEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now));
            A.CallTo(() => _entityDao.Save(A<TlsRptEntityHistoryState>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void HandleDomainCreatedThrowsIfEntityAlreadyExistsForDomain()
        {
            A.CallTo(() => _entityDao.Get(Id)).Returns(new TlsRptEntityHistoryState(Id));
            A.CallTo(() => _entityDao.Save(A<TlsRptEntityHistoryState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<RuaVerificationChangeFound>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandleTlsRptRecordsPolledAndUpdateWhenNoRecordsExistUpdatesHistoryState()
        {
            List<string> tlsRptRecords1 = CreateRecords().Records.Select(x => x.Record).ToList();

            A.CallTo(() => _entityDao.Get(Id)).Returns(new TlsRptEntityHistoryState(Id));

            TlsRptRecordsPolled tlsRptRecordsEvaluated =
                new TlsRptRecordsPolled(Id, CreateRecords(), null);

            await _historyEntity.Handle(tlsRptRecordsEvaluated);

            A.CallTo(() => _entityDao.Save(A<TlsRptEntityHistoryState>.That.Matches(_ =>
                _.TlsRptHistory.Count == 1 &&
                _.TlsRptHistory[0].EndDate == null &&
                _.TlsRptHistory[0].TlsRptRecords.SequenceEqual(tlsRptRecords1)))).MustHaveHappenedOnceExactly();

            A.CallTo(() => _tlsRptRuaService.Process(A<string>._, A<string>._)).MustHaveHappenedOnceExactly();

        }

        [Test]
        public async Task HandleTlsRptRecordsPolledAndExistingTlsRptRecordsHistoryStateNotUpdated()
        {
            A.CallTo(() => _entityDao.Get(Id)).Returns(new TlsRptEntityHistoryState(Id, new List<TlsRptHistoryRecord>
            {
                new TlsRptHistoryRecord(DateTime.UtcNow.AddDays(-1), null, CreateRecords().Records.Select(x => x.Record).ToList())
            }));

            TlsRptRecordsPolled tlsRptRecordsEvaluated = new TlsRptRecordsPolled(Id, CreateRecords(), null);

            await _historyEntity.Handle(tlsRptRecordsEvaluated);

            A.CallTo(() => _entityDao.Save(A<TlsRptEntityHistoryState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<RuaVerificationChangeFound>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandleTlsRptRecordsPolledAndNewTlsRptRecordUpdatesHistoryWhichHasOnePreviousRecord()
        {
            List<string> tlsRptRecords1 = CreateRecords().Records.Select(x => x.Record).ToList();
            List<string> tlsRptRecords2 = CreateRecords(record: "v=TLSRPTv1;rua=mailto:tlsrpt2@example.com").Records.Select(x => x.Record).ToList();

            A.CallTo(() => _entityDao.Get(Id)).Returns(new TlsRptEntityHistoryState(Id,
                new List<TlsRptHistoryRecord>
                {
                    new TlsRptHistoryRecord(DateTime.UtcNow.AddDays(-1), null, tlsRptRecords1
                    )
                }));

            TlsRptRecordsPolled tlsRptRecordsEvaluated =
                new TlsRptRecordsPolled(Id, CreateRecords(record: "v=TLSRPTv1;rua=mailto:tlsrpt2@example.com"), null)
                {
                    Timestamp = DateTime.UtcNow
                };

            await _historyEntity.Handle(tlsRptRecordsEvaluated);

            A.CallTo(() => _entityDao.Save(A<TlsRptEntityHistoryState>.That.Matches(_ =>
                _.TlsRptHistory.Count == 2 &&
                _.TlsRptHistory[0].EndDate == null &&
                _.TlsRptHistory[0].TlsRptRecords.SequenceEqual(tlsRptRecords2) &&
                _.TlsRptHistory[1].EndDate == tlsRptRecordsEvaluated.Timestamp &&
                _.TlsRptHistory[1].TlsRptRecords.SequenceEqual(tlsRptRecords1)))).MustHaveHappenedOnceExactly();

            A.CallTo(() => _tlsRptRuaService.Process(A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
        }


        [Test]
        public async Task HandleTlsRptRecordsPolledAndNewTlsRptRecordUpdatesHistoryWhichHasTwoPreviousRecord()
        {
            var tlsRptRecords1 = CreateRecords().Records.Select(x => x.Record).ToList();
            var tlsRptRecords2 = CreateRecords(record: "v=TLSRPTv1;rua=mailto:tlsrpt2@example.com").Records.Select(x => x.Record).ToList();
            var tlsRptRecords3 = CreateRecords(record: "v=TLSRPTv1;rua=mailto:tlsrpt3@example.com").Records.Select(x => x.Record).ToList();

            A.CallTo(() => _entityDao.Get(Id)).Returns(new TlsRptEntityHistoryState(Id,
                new List<TlsRptHistoryRecord>
                {
                    new TlsRptHistoryRecord(DateTime.UtcNow.AddDays(-2), null, tlsRptRecords2),
                    new TlsRptHistoryRecord(DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2), tlsRptRecords1)
                }));

            TlsRptRecordsPolled tlsRptRecordsEvaluated =
                new TlsRptRecordsPolled(Id, CreateRecords(record: "v=TLSRPTv1;rua=mailto:tlsrpt3@example.com"), null)
                {
                    Timestamp = DateTime.UtcNow
                };

            await _historyEntity.Handle(tlsRptRecordsEvaluated);

            A.CallTo(() => _entityDao.Save(A<TlsRptEntityHistoryState>.That.Matches(_ =>
                _.TlsRptHistory.Count == 3 &&
                _.TlsRptHistory[0].EndDate == null &&
                _.TlsRptHistory[0].TlsRptRecords.SequenceEqual(tlsRptRecords3) &&
                _.TlsRptHistory[1].EndDate == tlsRptRecordsEvaluated.Timestamp &&
                _.TlsRptHistory[1].TlsRptRecords.SequenceEqual(tlsRptRecords2) &&
                _.TlsRptHistory[2].TlsRptRecords.SequenceEqual(tlsRptRecords1)))).MustHaveHappenedOnceExactly();

            A.CallTo(() => _tlsRptRuaService.Process(A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
        }


        [Test]
        public async Task HandleTlsRptRecordsPolledWhenRecordsInDifferentOrderButSameRecordsNoUpdate()
        {
            TlsRptRecords record = CreateRecords(record: "one,two");

            TlsRptRecords record2 = CreateRecords(record: "two,one");

            A.CallTo(() => _entityDao.Get(Id)).Returns(new TlsRptEntityHistoryState(Id,
                new List<TlsRptHistoryRecord>
                {
                    new TlsRptHistoryRecord(DateTime.UtcNow.AddDays(-2), null, record.Records.Select(x => x.Record).ToList())
                }));

            TlsRptRecordsPolled tlsRptRecordsEvaluated =
                new TlsRptRecordsPolled(Id, record2, null)
                {
                    Timestamp = DateTime.UtcNow
                };

            await _historyEntity.Handle(tlsRptRecordsEvaluated);

            A.CallTo(() => _entityDao.Save(A<TlsRptEntityHistoryState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<RuaVerificationChangeFound>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandleTlsRptRecordsPolledWhenRecordsInSameOrderNoUpdate()
        {
            TlsRptRecords record = CreateRecords(record: "one,two");

            TlsRptRecords record2 = CreateRecords(record: "one,two");

            A.CallTo(() => _entityDao.Get(Id)).Returns(new TlsRptEntityHistoryState(Id,
                new List<TlsRptHistoryRecord>
                {
                    new TlsRptHistoryRecord(DateTime.UtcNow.AddDays(-2), null, record.Records.Select(x => x.Record).ToList())
                }));

            TlsRptRecordsPolled tlsRptRecordsEvaluated =
                new TlsRptRecordsPolled(Id, record2, null)
                {
                    Timestamp = DateTime.UtcNow
                };

            await _historyEntity.Handle(tlsRptRecordsEvaluated);

            A.CallTo(() => _entityDao.Save(A<TlsRptEntityHistoryState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<RuaVerificationChangeFound>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandleTlsRptRecordsPolledWhenNewRecords()
        {
            TlsRptRecords record = CreateRecords(record: "one,two");

            TlsRptRecords record2 = CreateRecords(record: "two,three");

            A.CallTo(() => _entityDao.Get(Id)).Returns(new TlsRptEntityHistoryState(Id,
                new List<TlsRptHistoryRecord>
                {
                    new TlsRptHistoryRecord(DateTime.UtcNow.AddDays(-2), null, new List<string> { record.Records[0].Record})
                }));

            TlsRptRecordsPolled tlsRptRecordsEvaluated =
                new TlsRptRecordsPolled(Id, record2, null)
                {
                    Timestamp = DateTime.UtcNow
                };

            await _historyEntity.Handle(tlsRptRecordsEvaluated);

            A.CallTo(() => _entityDao.Save(A<TlsRptEntityHistoryState>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _tlsRptRuaService.Process(A<string>._, A<string>._)).MustHaveHappenedTwiceExactly();
        }

        private static TlsRptRecords CreateRecords(string domain = Id, string record = "v=TLSRPTv1;rua=mailto:tlsrpt@example.com")
        {
            List<TlsRptRecord> records = new List<TlsRptRecord>();

            foreach (string tlsRptRecord in record.Split(','))
            {
                records.Add(new TlsRptRecord(domain, tlsRptRecord.Split(";").ToList(),
                    new List<Tag> {new MalformedTag("MalformedTag")}));
            }

            return new TlsRptRecords(domain, records,  100);
        }
    }
}