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
using Message = MailCheck.TlsRpt.Contracts.SharedDomain.Message;
using MessageDisplay = MailCheck.TlsRpt.Contracts.SharedDomain.MessageDisplay;
using MessageType = MailCheck.TlsRpt.Contracts.SharedDomain.MessageType;

namespace MailCheck.TlsRpt.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class AdvisoryChangedNotifierTests
    {
        private IMessageDispatcher _messageDispatcher;
        private ITlsRptEntityConfig _tlsRptEntityConfig;
        private IEqualityComparer<Message> _messageEqualityComparer;
        private AdvisoryChangedNotifier _advisoryChangedNotifier;

        [SetUp]
        public void SetUp()
        {
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _tlsRptEntityConfig = A.Fake<ITlsRptEntityConfig>();
            _messageEqualityComparer = new MessageEqualityComparer();

            _advisoryChangedNotifier = new AdvisoryChangedNotifier(_messageDispatcher, _tlsRptEntityConfig, _messageEqualityComparer);
        }

        [Test]
        public void RaisesAdvisorySustainedWhenNoChangeToExistingMessage()
        {
            Guid id = Guid.NewGuid();

            Message existingMessage = new Message(id, "testSource", MessageType.warning, "testText1", string.Empty);
            Message newMessage = new Message(id, "testSource text has changed!", MessageType.error, "testText2", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage), CreateTlsRptRecordsEvaluatedWithMessages(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisorySustained>.That.Matches(x => x.Messages.First().Text == "testText1"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenNoChangeSendsAdvisorySustained()
        {
            Guid messageId = Guid.NewGuid();

            Message existingMessage = new Message(messageId, "testSource", MessageType.info, "testText", string.Empty);
            Message newMessage = new Message(messageId, "testSource", MessageType.info, "testText", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage), CreateTlsRptRecordsEvaluatedWithMessages(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisorySustained>.That.Matches(x => x.Messages.First().Text == "testText"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesRemovedAndAddeddWhenMessageTypeChanges()
        {
            Message existingMessage = new Message(Guid.NewGuid(), "testSource", MessageType.info, "testText", string.Empty);
            Message newMessage = new Message(Guid.NewGuid(), "testSource", MessageType.error, "testText", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage), CreateTlsRptRecordsEvaluatedWithMessages(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>.That.Matches(x => x.Messages.First().MessageType == TlsRpt.Entity.Entity.Notifications.MessageType.error), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>.That.Matches(x => x.Messages.First().MessageType == TlsRpt.Entity.Entity.Notifications.MessageType.info), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisorySustained>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void RaisesRemovedAndAddeddWhenMessageTextChanges()
        {
            Message existingMessage = new Message(Guid.NewGuid(), "testSource", MessageType.info, "testText1", string.Empty);
            Message newMessage = new Message(Guid.NewGuid(), "testSource", MessageType.info, "testText2", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage), CreateTlsRptRecordsEvaluatedWithMessages(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>.That.Matches(x => x.Messages.First().Text == "testText2"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>.That.Matches(x => x.Messages.First().Text == "testText1"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisorySustained>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void RaisesRemovedAndAddeddWhenMessageChanges()
        {
            Message existingMessage = new Message(Guid.NewGuid(), "testSource", MessageType.info, "testText", string.Empty);
            Message newMessage = new Message(Guid.NewGuid(), "testSource", MessageType.info, "newTestText", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage), CreateTlsRptRecordsEvaluatedWithMessages(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>.That.Matches(x => x.Messages.First().Text == "newTestText"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>.That.Matches(x => x.Messages.First().Text == "testText"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisorySustained>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void RaisesAddedAndSustainedWhenMessageAddedAndExistingOneWithNoChange()
        {
            Guid messageId = Guid.NewGuid();

            Message existingMessage = new Message(messageId, "testSource1", MessageType.info, "testText1", string.Empty);

            Message newMessage1 = new Message(messageId, "testSource1", MessageType.info, "testText1", string.Empty);
            Message newMessage2 = new Message(Guid.NewGuid(), "testSource2", MessageType.info, "testText2", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage), CreateTlsRptRecordsEvaluatedWithMessages(newMessage1, newMessage2));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>.That.Matches(x => x.Messages.First().Text == "testText2"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisorySustained>.That.Matches(x => x.Messages.First().Text == "testText1"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesRemovedAndSustainedWhenMessageRemovedAndExistingOneWithNoChange()
        {
            Guid messageId = Guid.NewGuid();

            Message existingMessage1 = new Message(messageId, "testSource1", MessageType.info, "testText1", string.Empty);
            Message existingMessage2 = new Message(Guid.NewGuid(), "testSource2", MessageType.info, "testText2", string.Empty);

            Message newMessage = new Message(messageId, "testSource1", MessageType.info, "testText1", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithMessages(existingMessage1, existingMessage2), CreateTlsRptRecordsEvaluatedWithMessages(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>.That.Matches(x => x.Messages.First().Text == "testText2"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisorySustained>.That.Matches(x => x.Messages.First().Text == "testText1"), A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisorySustained>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesAddedAndRemovedWhenMessageChanges()
        {
            Message existingMessage = new Message(Guid.NewGuid(), "testSource", MessageType.info, "testText", string.Empty);
            Message newMessage = new Message(Guid.NewGuid(), "testSource", MessageType.error, "testText", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithRecords(existingMessage), CreateTlsRptRecordsEvaluatedWithRecords(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>.That.Matches(x => x.Messages.First().MessageType == TlsRpt.Entity.Entity.Notifications.MessageType.error), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>.That.Matches(x => x.Messages.First().MessageType == TlsRpt.Entity.Entity.Notifications.MessageType.info), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesAddedWhenMessageChanges()
        {
            Message existingMessage = new Message(Guid.NewGuid(), "testSource", MessageType.info, "testText1", string.Empty);
            Message newMessage = new Message(Guid.NewGuid(), "testSource", MessageType.info, "testText2", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithRecords(existingMessage), CreateTlsRptRecordsEvaluatedWithRecords(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>.That.Matches(x => x.Messages.First().Text == "testText2"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>.That.Matches(x => x.Messages.First().Text == "testText1"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void RaisesAddedWhenMessageDisplayTypeChanges()
        {
            Message existingMessage = new Message(Guid.NewGuid(), "testSource", MessageType.info, "testText", string.Empty, MessageDisplay.Prompt);
            Message newMessage = new Message(Guid.NewGuid(), "testSource", MessageType.info, "testText", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithRecords(existingMessage), CreateTlsRptRecordsEvaluatedWithRecords(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>.That.Matches(x => x.Messages.First().MessageDisplay == TlsRpt.Entity.Entity.Notifications.MessageDisplay.Standard), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void RaisesAddedWhenMessageAdded()
        {
            Guid messageId = Guid.NewGuid();

            Message existingMessage = new Message(messageId,"testSource1", MessageType.info, "testText1", string.Empty);

            Message newMessage1 = new Message(messageId,"testSource1", MessageType.info, "testText1", string.Empty);
            Message newMessage2 = new Message(Guid.NewGuid(),"testSource2", MessageType.info, "testText2", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithRecords(existingMessage), CreateTlsRptRecordsEvaluatedWithRecords(newMessage1, newMessage2));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>.That.Matches(x => x.Messages.First().Text == "testText2"), A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void RaisesRemovedWhenMessageRemoved()
        {
            Guid messageId = Guid.NewGuid();

            Message existingMessage1 = new Message(messageId,"testSource1", MessageType.info, "testText1", string.Empty);
            Message existingMessage2 = new Message(Guid.NewGuid(),"testSource2", MessageType.info, "testText2", string.Empty);

            Message newMessage = new Message(messageId,"testSource1", MessageType.info, "testText1", string.Empty);

            _advisoryChangedNotifier.Handle(CreateEntityStateWithRecords( existingMessage1, existingMessage2), CreateTlsRptRecordsEvaluatedWithRecords(newMessage));

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();

            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageDispatcher.Dispatch(A<TlsRptAdvisoryRemoved>.That.Matches(x => x.Messages.First().Text == "testText2"), A<string>._)).MustHaveHappenedOnceExactly();
        }

        private TlsRptEntityState CreateEntityStateWithMessages(params Message[] messages)
        {
            TlsRptEntityState entityState = new TlsRptEntityState("", 0, new TlsRptState(), DateTime.MaxValue)
            {
                Messages = messages.ToList()
            };

            return entityState;
        }

        private TlsRptRecordsEvaluated CreateTlsRptRecordsEvaluatedWithMessages(params Message[] messages)
        {
            TlsRptRecordsEvaluated recordsEvaluated = new TlsRptRecordsEvaluated("", null,  messages.ToList(), DateTime.MinValue);
            return recordsEvaluated;
        }
        
        private static TlsRptEntityState CreateEntityStateWithRecords(params Message[] messages)
        {
            string domain = "abc.com";

            string record = "v=TLSRPTv1;rua=mailto:tlsrpt@example.com";

            TlsRptEntityState state = new TlsRptEntityState(domain, 1, TlsRptState.Created, DateTime.UtcNow);
            state.TlsRptRecords = CreateTlsRptRecords(domain, record: record);
            state.Messages = messages.ToList();
            return state;
        }

        private static TlsRptRecords CreateTlsRptRecords(string domain = "abc.com",
            string record = "v=TLSRPTv1;rua=mailto:tlsrpt@example.com")
        {
            return new TlsRptRecords(domain, new List<TlsRptRecord>
            {
                new TlsRptRecord(domain, record.Split(";").ToList(), new List<Tag> {new UnknownTag("UnknownTag")})
            }, 100);
        }

        private TlsRptRecordsEvaluated CreateTlsRptRecordsEvaluatedWithRecords(params Message[] messages)
        {
            TlsRptRecordsEvaluated recordsEvaluated = new TlsRptRecordsEvaluated("", null,  messages.ToList(), DateTime.MinValue);
            return recordsEvaluated;
        }
    }
}
