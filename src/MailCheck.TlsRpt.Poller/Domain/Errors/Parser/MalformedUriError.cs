using System;

namespace MailCheck.TlsRpt.Poller.Domain.Errors.Parser
{
    public class MalformedUriError : MalformItemError
    {
        private static readonly Guid _id = Guid.Parse("c29a2bed-3b2c-4043-bb63-78a0c01eb0ee");

        public MalformedUriError(string uriValue) 
            : base(_id, "uri", uriValue, TlsRptParserMarkDown.MalformedUriError)
        {
            
        }
    }
}