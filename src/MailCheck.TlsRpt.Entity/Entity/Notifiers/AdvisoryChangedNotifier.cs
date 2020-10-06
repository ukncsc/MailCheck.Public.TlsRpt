using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.Evaluator;
using MailCheck.TlsRpt.Entity.Config;
using MailCheck.TlsRpt.Entity.Entity.Notifications;
using Message = MailCheck.TlsRpt.Contracts.SharedDomain.Message;
using MessageDisplay = MailCheck.TlsRpt.Contracts.SharedDomain.MessageDisplay;

namespace MailCheck.TlsRpt.Entity.Entity.Notifiers
{
    public class AdvisoryChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly ITlsRptEntityConfig _tlsRptEntityConfig;
        private readonly IEqualityComparer<Message> _messageEqualityComparer;

        public AdvisoryChangedNotifier(IMessageDispatcher dispatcher, ITlsRptEntityConfig tlsRptEntityConfig, IEqualityComparer<Message> messageEqualityComparer)
        {
            _dispatcher = dispatcher;
            _tlsRptEntityConfig = tlsRptEntityConfig;
            _messageEqualityComparer = messageEqualityComparer;
        }

        public void Handle(TlsRptEntityState state, Common.Messaging.Abstractions.Message message)
        {
            if (message is TlsRptRecordsEvaluated evaluationResult)
            {
                List<Message> currentMessages = state.Messages.Where(_ => _.MessageDisplay != MessageDisplay.Prompt).ToList();
             
                List<Message> newMessages = evaluationResult.Messages.Where(_ => _.MessageDisplay != MessageDisplay.Prompt).ToList();
               
                List<Message> addedMessages = newMessages.Except(currentMessages, _messageEqualityComparer).ToList();
                if (addedMessages.Any())
                {
                    TlsRptAdvisoryAdded advisoryAdded = new TlsRptAdvisoryAdded(state.Id, addedMessages.Select(x => new AdvisoryMessage((MessageType) x.MessageType, x.Text, (Notifications.MessageDisplay) x.MessageDisplay)).ToList());
                    _dispatcher.Dispatch(advisoryAdded, _tlsRptEntityConfig.SnsTopicArn);
                }

                List<Message> removedMessages = currentMessages.Except(newMessages, _messageEqualityComparer).ToList();
                if (removedMessages.Any())
                {
                    TlsRptAdvisoryRemoved advisoryRemoved = new TlsRptAdvisoryRemoved(state.Id, removedMessages.Select(x => new AdvisoryMessage((MessageType) x.MessageType, x.Text, (Notifications.MessageDisplay) x.MessageDisplay)) .ToList());
                    _dispatcher.Dispatch(advisoryRemoved, _tlsRptEntityConfig.SnsTopicArn);
                }
            }
        }
    }
}