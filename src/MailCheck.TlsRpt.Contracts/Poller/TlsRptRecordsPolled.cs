using System.Collections.Generic;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.TlsRpt.Contracts.Poller
{
    public class TlsRptRecordsPolled : Message
    {
        public TlsRptRecordsPolled(string id, TlsRptRecords tlsRptRecords, List<SharedDomain.Message> messages) : base(id)
        {
            TlsRptRecords = tlsRptRecords;
            Messages = messages ?? new List<SharedDomain.Message>();
        }

        public TlsRptRecords TlsRptRecords { get; }
        public List<SharedDomain.Message> Messages { get; }
    }
}
