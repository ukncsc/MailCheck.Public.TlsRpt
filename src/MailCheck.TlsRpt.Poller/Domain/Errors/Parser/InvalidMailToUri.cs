using System;

namespace MailCheck.TlsRpt.Poller.Domain.Errors.Parser
{
    public class InvalidMailToUri : InvalidItemValueError
    {
        private static readonly Guid _Id = Guid.Parse("8fea8be1-fc06-465b-afb0-e4590fae702a");

        public InvalidMailToUri(string mailToUriValue)
            : base(_Id, "mailcheck.tlsrpt.invalidMailToUri", "mailto", mailToUriValue)
        {
        }
    }
}
