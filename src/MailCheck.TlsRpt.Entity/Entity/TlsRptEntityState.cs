using System;
using System.Collections.Generic;
using MailCheck.TlsRpt.Contracts.Entity;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using Evnt = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.TlsRpt.Entity.Entity
{
    public class TlsRptEntityState 
    {
        public TlsRptEntityState(string id, int version, TlsRptState tlsRptState, DateTime created) 
        {
            Id = id;
            Version = version;
            TlsRptState = tlsRptState;
            Created = created;
            Messages = new List<Message>();
        }

        public virtual string Id { get; }

        public virtual int Version { get; set; }

        public virtual TlsRptState TlsRptState { get; set; }

        public virtual DateTime Created { get; }

        public virtual TlsRptRecords TlsRptRecords { get; set; }

        public virtual List<Message> Messages { get; set; }

        public virtual DateTime? LastUpdated { get; set; }
        
        public Evnt UpdatePollPending()
        {
            TlsRptState = TlsRptState.PollPending;

            return new TlsRptPollPending(Id);
        }
        
        public Evnt UpdateTlsRptEvaluation(TlsRptRecords tlsRptRecords, List<Message> messages, DateTime lastUpdated)
        {
            TlsRptRecords = tlsRptRecords;
            LastUpdated = lastUpdated;
            Messages = messages;
            LastUpdated = lastUpdated;
            TlsRptState = TlsRptState.Evaluated;

            return new TlsRptRecordEvaluationsChanged(Id, tlsRptRecords, messages);
        }
    }
}
