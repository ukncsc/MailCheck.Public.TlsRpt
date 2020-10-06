using System;
using System.Collections.Generic;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.TlsRpt.Contracts.Evaluator
{
    public class TlsRptRecordsEvaluated : Message
    {
        public TlsRptRecordsEvaluated(string id, TlsRptRecords records, List<SharedDomain.Message> messages, DateTime lastUpdated) : base(id)
        {
            Records = records;
            Messages = messages;
            LastUpdated = lastUpdated;
        }

        public TlsRptRecords Records { get; }
        
        public List<SharedDomain.Message> Messages { get; }

        public DateTime LastUpdated { get; }
    }
}
