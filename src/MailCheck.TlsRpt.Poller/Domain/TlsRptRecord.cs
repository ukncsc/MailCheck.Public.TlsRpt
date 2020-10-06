using System.Collections.Generic;

namespace MailCheck.TlsRpt.Poller.Domain
{
    public class TlsRptRecord
    {
        public TlsRptRecord(string domain, List<string> recordsParts, List<Tag> tags)
        {
            Domain = domain;
            RecordsParts = recordsParts;
            Tags = tags;
        }

        public string Domain { get; }
        public List<string> RecordsParts { get; }
        public List<Tag> Tags { get; }
    }
}