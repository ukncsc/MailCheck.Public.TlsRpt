using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using Newtonsoft.Json;

namespace MailCheck.TlsRpt.Poller.Domain
{
    public class TlsRptPollResult : Message
    {
        [JsonConstructor]
        public TlsRptPollResult(string id, TlsRptRecords tlsRptRecords, List<Error> errors) 
            : base(id)
        {
            TlsRptRecords = tlsRptRecords;
            Errors = errors ?? new List<Error>();
        }

        public TlsRptPollResult(TlsRptRecords rptRecordInfos, List<Error> errors)
         : this(rptRecordInfos.Domain, rptRecordInfos, errors)
        {
            
        }

        public TlsRptPollResult(string id, params Error[] errors)
         : this(id, null, errors.ToList())
        {
        }

        public TlsRptRecords TlsRptRecords { get; }
        public List<Error> Errors { get; }
    }
}