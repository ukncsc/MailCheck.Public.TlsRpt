using System.Collections.Generic;

namespace MailCheck.TlsRpt.Poller.Domain
{
    public class TlsRptRecordInfos
    {
        private TlsRptRecordInfos(string domain, List<TlsRptRecordInfo> recordsInfos, int messageSize, Error error)
        {
            Domain = domain;
            RecordsInfos = recordsInfos ?? new List<TlsRptRecordInfo>();
            MessageSize = messageSize;
            Error = error;
        }

        public TlsRptRecordInfos(string domain, List<TlsRptRecordInfo> recordsInfos, int messageSize)
          : this(domain, recordsInfos, messageSize, null )
        {
        }

        public TlsRptRecordInfos(string domain, Error error, int messageSize)
            : this(domain, null, messageSize, error)
        {
        }

        public string Domain { get; }
        public List<TlsRptRecordInfo> RecordsInfos { get; }
        public int MessageSize { get; }
        public Error Error { get; }
        public bool HasError => Error != null;
    }
}