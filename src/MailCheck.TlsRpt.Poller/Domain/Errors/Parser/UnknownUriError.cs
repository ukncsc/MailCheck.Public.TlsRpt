using System;

namespace MailCheck.TlsRpt.Poller.Domain.Errors.Parser
{
    public class UnknownUriError : UnknownItemError
    {
        private static readonly Guid _id = Guid.Parse("aad4acd9-1134-4f20-89b0-5fe7fa75112d");

        public UnknownUriError(string uriKey, string uriValue) 
            : base(_id, "mailcheck.tlsrpt.unknownUri", "uri", uriKey, uriValue, TlsRptParserMarkDown.UnknownUriError)
        {
            
        }
    }
}