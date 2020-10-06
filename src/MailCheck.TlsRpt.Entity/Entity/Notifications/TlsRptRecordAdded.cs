using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.TlsRpt.Entity.Entity.Notifications
{
    public class TlsRptRecordAdded : Message
    {
        public TlsRptRecordAdded(string id, List<string> records) : base(id)
        {
            Records = records;
        }

        public List<string> Records { get; }
    }
}