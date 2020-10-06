using MailCheck.TlsRpt.Contracts.SharedDomain;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.TlsRpt.Contracts.Entity
{
    public class TlsRptPollPending : Message
    {
        public TlsRptPollPending(string id) : base(id)
        {
        }

        public TlsRptState State => TlsRptState.PollPending;
    }
}
