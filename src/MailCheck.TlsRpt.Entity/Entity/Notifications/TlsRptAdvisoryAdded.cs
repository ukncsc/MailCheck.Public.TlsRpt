using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.TlsRpt.Entity.Entity.Notifications
{
    public class TlsRptAdvisoryAdded : Message
    {
        public TlsRptAdvisoryAdded(string id, List<AdvisoryMessage> messages) : base(id)
        {
            Messages = messages;
        }

        public List<AdvisoryMessage> Messages { get; }
    }
}