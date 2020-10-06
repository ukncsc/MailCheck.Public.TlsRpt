using System;

namespace MailCheck.TlsRpt.Poller.Parsing
{
    public interface IMailToUriValidator : IUriValidator
    {

    }

    public class MailToUriValidator : IMailToUriValidator
    {
        public bool IsValidUri(string uri)
        {
            return uri != null &&
                   Uri.TryCreate(uri, UriKind.Absolute, out Uri parsedUri) &&
                   parsedUri.Scheme == "mailto" &&
                   !string.IsNullOrEmpty(parsedUri.UserInfo);

        }
    }
}