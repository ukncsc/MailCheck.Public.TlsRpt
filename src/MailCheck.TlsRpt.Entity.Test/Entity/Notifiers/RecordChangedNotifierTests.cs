using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.Evaluator;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using MailCheck.TlsRpt.Entity.Config;
using MailCheck.TlsRpt.Entity.Entity;
using MailCheck.TlsRpt.Entity.Entity.Notifications;
using MailCheck.TlsRpt.Entity.Entity.Notifiers;
using NUnit.Framework;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.TlsRpt.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class RecordChangedNotifierTests
    {
        private IMessageDispatcher _messageDispatcher;
        private ITlsRptEntityConfig _tlsRptEntityConfig;
        private RecordChangedNotifier _recordChangedNotifier;

        [SetUp]
        public void SetUp()
        {
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _tlsRptEntityConfig = A.Fake<ITlsRptEntityConfig>();

            _recordChangedNotifier = new RecordChangedNotifier(_messageDispatcher, _tlsRptEntityConfig);
        }

        [Test]
        public void DoesNotNotifyWhenNoChanges()
        {
            _recordChangedNotifier.Handle(CreateEntityState("testRecord"), CreateTlsRptRecordsEvaluated("testRecord"));

            A.CallTo(() => _messageDispatcher.Dispatch(A<Message>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void NotifiesWhenRecordChanges()
        {
            _recordChangedNotifier.Handle(CreateEntityState("testRecord1"), CreateTlsRptRecordsEvaluated("testRecord2"));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptRecordAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptRecordAdded>.That.Matches(x => x.Records.First() == "testRecord2"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptRecordRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptRecordRemoved>.That.Matches(x => x.Records.First() == "testRecord1"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NotifiesWhenRecordAdded()
        {
            _recordChangedNotifier.Handle(CreateEntityState("testRecord1"), CreateTlsRptRecordsEvaluated("testRecord1", "testRecord2"));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptRecordAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptRecordAdded>.That.Matches(x => x.Records.First() == "testRecord2"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptRecordRemoved>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void NotifiesWhenRecordRemoved()
        {
            _recordChangedNotifier.Handle(CreateEntityState("testRecord1", "testRecord2"), CreateTlsRptRecordsEvaluated("testRecord1"));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptRecordAdded>._, A<string>._)).MustNotHaveHappened();

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptRecordRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptRecordRemoved>.That.Matches(x => x.Records.First() == "testRecord2"), A<string>._)).MustHaveHappenedOnceExactly();
        }
        

        private TlsRptEntityState CreateEntityState(params string[] recordStrings)
        {
            IEnumerable<TlsRptRecord> tlsRptRecords = recordStrings.Select(x => new TlsRptRecord(x, x.Split(";").ToList(), null));
            TlsRptEntityState entityState = new TlsRptEntityState("", 0, new TlsRptState(), DateTime.MaxValue)
            {
                TlsRptRecords = new TlsRptRecords("abc.com", tlsRptRecords.ToList(), 0)
            };

            return entityState;
        }

        private TlsRptRecordsEvaluated CreateTlsRptRecordsEvaluated(params string[] recordStrings)
        {
            IEnumerable<TlsRptRecord> records =
                recordStrings.Select(x => new TlsRptRecord(x, x.Split(";").ToList(), null));
            TlsRptRecordsEvaluated recordsEvaluated = new TlsRptRecordsEvaluated("",
                new TlsRptRecords("", records.ToList(), 0), null, DateTime.MinValue);
            return recordsEvaluated;
        }
    }
}
