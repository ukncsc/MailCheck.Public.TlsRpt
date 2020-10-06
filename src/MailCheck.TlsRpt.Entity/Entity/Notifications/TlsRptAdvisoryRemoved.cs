using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.TlsRpt.Entity.Entity.Notifications
{
    public class TlsRptAdvisoryRemoved : Message
    {
        public TlsRptAdvisoryRemoved(string id, List<AdvisoryMessage> messages) : base(id)
        {
            Messages = messages;
        }

        public List<AdvisoryMessage> Messages { get; }
    }
}
