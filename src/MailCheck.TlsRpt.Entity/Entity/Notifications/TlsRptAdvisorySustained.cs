using System;
using System.Collections.Generic;
using System.Text;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.TlsRpt.Entity.Entity.Notifications
{
    public class TlsRptAdvisorySustained : Message
    {
        public TlsRptAdvisorySustained(string id, List<AdvisoryMessage> messages) : base(id)
        {
            Messages = messages;
        }

        public List<AdvisoryMessage> Messages { get; }
    }
}
