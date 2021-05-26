using System.Collections.Generic;

namespace MailCheck.TlsRpt.Poller.Domain
{
    public class TlsRptRecordInfos
    {
        private TlsRptRecordInfos(string domain, List<TlsRptRecordInfo> recordsInfos, int messageSize, Error error, string nameServer, string auditTrail)
        {
            Domain = domain;
            RecordsInfos = recordsInfos ?? new List<TlsRptRecordInfo>();
            MessageSize = messageSize;
            Error = error;
            NameServer = nameServer;
            AuditTrail = auditTrail;
        }

        public TlsRptRecordInfos(string domain, Error error, int messageSize, string nameServer, string auditTrail)
            : this(domain, null, messageSize, error, nameServer, auditTrail)
        {
        }

        public TlsRptRecordInfos(string domain, List<TlsRptRecordInfo> recordsInfos, int messageSize)
            : this(domain, recordsInfos, messageSize, null, null, null)
        {
        }

        public string Domain { get; }
        public List<TlsRptRecordInfo> RecordsInfos { get; }
        public int MessageSize { get; }
        public Error Error { get; }
        public string NameServer { get; }
        public string AuditTrail { get; }
        public bool HasError => Error != null;
    }
}