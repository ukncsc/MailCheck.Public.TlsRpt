using System.Collections.Generic;
using System.Linq;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Domain.Errors.Parser;
using MailCheck.TlsRpt.Poller.Rules;

namespace MailCheck.TlsRpt.Poller.Parsing
{
    public class RuaParser : ITagParser
    {
        private const string UriDelimiter = ",";
        private const string UriTypeDelimiter = ":";

        private readonly Dictionary<string, IUriParser> _parsers;

        public RuaParser(IEnumerable<IUriParser> parsers)
        {
            _parsers = parsers.ToDictionary(_ => _.UriType, _ => _);
        }

        public EvaluationResult<Tag> Parse(List<Tag> tags, string record, string token, string tagKey, string tagValue)
        {
            string[] stringUris = tagValue.Split(UriDelimiter);
            List<Uri> uris = new List<Uri>();
            List<Error> errors = new List<Error>();
            foreach (string uri in stringUris.Select(x=>x.Trim()))
            {
                string[] uriParts = uri.Split(UriTypeDelimiter);
                if (uriParts.Length == 2)
                {
                    string uriKey = uriParts[0];
                    if (_parsers.TryGetValue(uriKey, out IUriParser parser))
                    {
                        var uriResult = parser.Parse(uri);

                        uris.Add(uriResult.Item);
                        errors.AddRange(uriResult.Errors);
                    }
                    else
                    {
                        uris.Add(new UnknownUri(uri));
                        errors.Add(new UnknownUriError(uriKey, uriParts[1]));
                    }
                }
                else
                {
                    uris.Add(new MalformedUri(uri));
                    errors.Add(new MalformedUriError(uri));
                }
            }

            return new EvaluationResult<Tag>(new RuaTag(token, uris), errors);
        }

        public string TagType => "rua";
        public int MaxOccurrences => 1;
    }
}