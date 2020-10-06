using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.Evaluator;
using MailCheck.TlsRpt.Entity.Config;
using MailCheck.TlsRpt.Entity.Entity.Notifications;

namespace MailCheck.TlsRpt.Entity.Entity.Notifiers
{
    public class RecordChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly ITlsRptEntityConfig _tlsRptEntityConfig;

        public RecordChangedNotifier(IMessageDispatcher dispatcher, ITlsRptEntityConfig tlsRptEntityConfig)
        {
            _dispatcher = dispatcher;
            _tlsRptEntityConfig = tlsRptEntityConfig;
        }

        public void Handle(TlsRptEntityState state, Message message)
        {
            if (message is TlsRptRecordsEvaluated recordsEvaluated)
            {
                List<string> currentRecords = state.TlsRptRecords?.Records.Select(x => x.Record?.Replace("; ", ";")).ToList() ?? new List<string>();
                List<string> newRecords = recordsEvaluated.Records?.Records.Select(x => x.Record?.Replace("; ", ";")).ToList() ?? new List<string>();

                List<string> addedRecords = newRecords.Except(currentRecords).ToList();
                if (addedRecords.Any())
                {
                    TlsRptRecordAdded advisoryAdded = new TlsRptRecordAdded(state.Id, addedRecords);
                    _dispatcher.Dispatch(advisoryAdded, _tlsRptEntityConfig.SnsTopicArn);
                }

                List<string> removedRecords = currentRecords.Except(newRecords).ToList();
                if (removedRecords.Any())
                {
                    TlsRptRecordRemoved advisoryRemoved = new TlsRptRecordRemoved(state.Id, removedRecords);
                    _dispatcher.Dispatch(advisoryRemoved, _tlsRptEntityConfig.SnsTopicArn);
                }
            }
        }
    }
}