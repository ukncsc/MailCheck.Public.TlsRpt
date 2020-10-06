using System;

namespace MailCheck.TlsRpt.Poller.Parsing
{
    public interface IHttpsUriValidator : IUriValidator
    {

    }

    public class HttpsUriValidator : IHttpsUriValidator
    {
        public bool IsValidUri(string uri)
        {
            return uri != null &&
                   Uri.TryCreate(uri, UriKind.Absolute, out Uri parsedUri) &&
                   parsedUri.Scheme == "https" &&
                   parsedUri.DnsSafeHost.Contains(".");
        }
    }
}