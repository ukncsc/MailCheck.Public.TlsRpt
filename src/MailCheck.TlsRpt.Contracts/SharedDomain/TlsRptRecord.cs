using System.Collections.Generic;

namespace MailCheck.TlsRpt.Contracts.SharedDomain
{
    public class TlsRptRecord
    {
        public TlsRptRecord(string domain, List<string> recordsParts, List<Tag> tags)
        {
            Domain = domain;
            Record = string.Join(string.Empty, recordsParts);
            RecordsParts = recordsParts;
            Tags = tags;
        }

        public string Domain { get; }
        public string Record { get; }
        public List<string> RecordsParts { get; }
        public List<Tag> Tags { get; }
    }
}