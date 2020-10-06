using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Rules;

namespace MailCheck.TlsRpt.Poller.Parsing
{
    public interface IUriParser
    {
        EvaluationResult<Uri> Parse(string uriToken);
        string UriType { get; }
    }
}