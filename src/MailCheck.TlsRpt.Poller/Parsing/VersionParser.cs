using System.Collections.Generic;
using System.Linq;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Domain.Errors.Parser;
using MailCheck.TlsRpt.Poller.Rules;

namespace MailCheck.TlsRpt.Poller.Parsing
{
    public class VersionParser : ITagParser
    {
        private const string ValidValue = "v=TLSRPTv1";

        public EvaluationResult<Tag> Parse(List<Tag> tags, string record, string token, string tagKey, string tagValue)
        {
            VersionTag versionTag = new VersionTag(token, tagValue);
            List<Error> errors = new List<Error>();
            
            int tagInstanceCount = tags.Count(_ => _.GetType() == typeof(VersionTag));

            if (!record.Trim().StartsWith(ValidValue) && tagInstanceCount == 0)
            {
                errors.Add(new InvalidVersionTagError());
            }

            return new EvaluationResult<Tag>(versionTag, errors);
        }

        public string TagType => "v";
        public int MaxOccurrences => 1;
    }
}