using System.Collections.Generic;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.TlsRpt.Contracts.Entity
{
    public class TlsRptRecordEvaluationsChanged : Message
    {
        public TlsRptRecordEvaluationsChanged(string id, TlsRptRecords records, List<SharedDomain.Message> messages) : base(id)
        {
            Records = records;

            Messages = messages;
        }

        public TlsRptRecords Records { get; }

        public List<SharedDomain.Message> Messages { get; }

        public TlsRptState State => TlsRptState.Evaluated;
    }
}