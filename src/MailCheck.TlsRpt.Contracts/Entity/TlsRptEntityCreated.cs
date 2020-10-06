using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.SharedDomain;

namespace MailCheck.TlsRpt.Contracts.Entity
{
    public class TlsRptEntityCreated : VersionedMessage
    {
        public TlsRptEntityCreated(string id, int version) 
            : base(id, version)
        {
        }

        public TlsRptState State => TlsRptState.Created;
    }
}
