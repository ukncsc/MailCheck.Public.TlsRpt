using System.Collections.Generic;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Rules;

namespace MailCheck.TlsRpt.Poller.Parsing
{
    public interface ITagParser
    {
        EvaluationResult<Tag> Parse(List<Tag> tags, string record, string token, string tagKey, string tagValue);
        string TagType { get; }
        int MaxOccurrences { get; }
    }
}