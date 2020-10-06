using System.Collections.Generic;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Domain.Errors.Parser;
using MailCheck.TlsRpt.Poller.Rules;

namespace MailCheck.TlsRpt.Poller.Parsing
{
    public class HttpsRuaParser : IUriParser
    {
        private readonly IUriValidator _uriValidator;

        public HttpsRuaParser(IHttpsUriValidator uriValidator)
        {
            _uriValidator = uriValidator;
        }
        
        public EvaluationResult<Uri> Parse(string uriToken)
        {
            Uri uri = new MailToUri(uriToken);
            List<Error> errors = new List<Error>();
            if (!_uriValidator.IsValidUri(uriToken))
            {
                errors.Add(new InvalidHttpsUri(uriToken));
            }

            return new EvaluationResult<Uri>(uri, errors);
        }

        public string UriType => "https";
    }
}