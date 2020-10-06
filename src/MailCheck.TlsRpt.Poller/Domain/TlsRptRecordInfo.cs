using System.Collections.Generic;

namespace MailCheck.TlsRpt.Poller.Domain
{
    public class TlsRptRecordInfo
    {
        public TlsRptRecordInfo(string domain, List<string> recordParts)
        {
            Domain = domain;
            RecordParts = recordParts;
            Record = string.Join(string.Empty, recordParts);
        }

        public string Domain { get; }
        public List<string> RecordParts { get; }
        public string Record { get; }
    }
}