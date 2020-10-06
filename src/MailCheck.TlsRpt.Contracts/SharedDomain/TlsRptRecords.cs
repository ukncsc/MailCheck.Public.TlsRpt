using System.Collections.Generic;

namespace MailCheck.TlsRpt.Contracts.SharedDomain
{
    public class TlsRptRecords
    {
        public TlsRptRecords(string domain, List<TlsRptRecord> records, int messageSize)
        {
            Domain = domain;
            Records = records;
            MessageSize = messageSize;
        }

        public string Domain { get; }
        public List<TlsRptRecord> Records { get; }
        public int MessageSize { get; }
    }
}