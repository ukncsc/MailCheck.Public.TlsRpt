using System.Collections.Generic;
using System.Linq;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Domain.Errors.Parser;
using MailCheck.TlsRpt.Poller.Rules;

namespace MailCheck.TlsRpt.Poller.Parsing
{
    public interface ITlsRptRecordParser
    {
        EvaluationResult<TlsRptRecord> Parse(TlsRptRecordInfo tlsRptRecordInfo);
    }

    public class TlsRptRecordParser : ITlsRptRecordParser
    {
        private const string TagDelimiter = ";";
        private const string TagPartDelimiter = "=";

        private readonly Dictionary<string, ITagParser> _parsers;

        public TlsRptRecordParser(IEnumerable<ITagParser> parsers)
        {
            _parsers = parsers.ToDictionary(_ => _.TagType, _ => _);
        }

        public EvaluationResult<TlsRptRecord> Parse(TlsRptRecordInfo tlsRptRecordInfo)
        {
            List<Tag> tags = new List<Tag>();
            List<Error> errors = new List<Error>();

            string[] tokens = tlsRptRecordInfo.Record
                .Split(TagDelimiter)
                .Select(_ => _.Trim())
                .Where(_ => !string.IsNullOrWhiteSpace(_))
                .ToArray();

            foreach (string token in tokens)
            {
                string[] tagParts = token.Split(TagPartDelimiter);

                if (tagParts.Length == 2)
                {
                    string tagKey = tagParts[0].ToLower();
                    string tagValue = tagParts[1];

                    if(_parsers.TryGetValue(tagKey, out ITagParser tagParser))
                    {
                        EvaluationResult<Tag> tag = tagParser.Parse(tags, tlsRptRecordInfo.Record, token, tagKey, tagValue);

                        int tagInstanceCount = tags.Count(_ => _.GetType() == tag.Item.GetType());

                        if (tagInstanceCount == tagParser.MaxOccurrences)
                        {
                            tag.Errors.Add(new MaxOccurrencesExceededError(tagKey, tagParser.MaxOccurrences, tagInstanceCount));
                        }

                        tags.Add(tag.Item);
                        errors.AddRange(tag.Errors);
                    }
                    else
                    {
                        tags.Add(new UnknownTag(token));
                        errors.Add(new UnknownTagError(tagKey, tagValue));
                    }
                }
                else
                {
                    tags.Add(new MalformedTag(token));
                    errors.Add(new MalformedTagError(token));
                }
            }

            TlsRptRecord tlsRptRecord = new TlsRptRecord(tlsRptRecordInfo.Domain, tlsRptRecordInfo.RecordParts, tags);
            return new EvaluationResult<TlsRptRecord>(tlsRptRecord, errors);
        }
    }
}

