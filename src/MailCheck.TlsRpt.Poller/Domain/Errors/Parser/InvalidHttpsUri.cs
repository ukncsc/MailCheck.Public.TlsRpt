using System;

namespace MailCheck.TlsRpt.Poller.Domain.Errors.Parser
{
    public class InvalidHttpsUri : InvalidItemValueError
    {
        private static readonly Guid _Id = Guid.Parse("52d3ebe6-0fa7-4364-b644-6ec6c337a880");

        public InvalidHttpsUri(string httpsUriValue)
            : base(_Id, "https", httpsUriValue)
        {
        }
    }
}