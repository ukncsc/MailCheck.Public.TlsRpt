using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.TlsRpt.Contracts.Scheduler
{
    public class TlsRptRecordExpired : Message
    {
        public TlsRptRecordExpired(string id)
            : base(id) { }
    }
}
