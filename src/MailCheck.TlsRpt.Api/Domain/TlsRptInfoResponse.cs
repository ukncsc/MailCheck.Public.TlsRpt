using System;
using System.Collections.Generic;
using MailCheck.TlsRpt.Contracts.SharedDomain;

namespace MailCheck.TlsRpt.Api.Domain
{
    public class TlsRptInfoResponse
    {
        public TlsRptInfoResponse(string id, TlsRptState tlsRptState, TlsRptRecords tlsRptRecords = null, List<Message> messages = null, DateTime? lastUpdated = null)
        {
            Id = id;
            Status = tlsRptState;
            TlsRptRecords = tlsRptRecords;
            Messages = messages;
            LastUpdated = lastUpdated;
        }

        public string Id { get; }

        public TlsRptState Status { get; }

        public TlsRptRecords TlsRptRecords { get; }

        public List<Message> Messages { get; }

        public DateTime? LastUpdated { get; }
    }
}
