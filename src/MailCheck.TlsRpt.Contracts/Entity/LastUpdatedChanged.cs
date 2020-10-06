using System;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.TlsRpt.Contracts.Entity
{
    public class LastUpdatedChanged : Message
    {
        public LastUpdatedChanged(string id, DateTime lastUpdated) 
            : base(id)
        {
            LastUpdated = lastUpdated;
        }

        public DateTime LastUpdated { get; }

        public TlsRptState State => TlsRptState.Unchanged;
    }
}