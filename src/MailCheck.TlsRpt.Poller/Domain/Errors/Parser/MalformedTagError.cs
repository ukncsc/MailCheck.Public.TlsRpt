using System;

namespace MailCheck.TlsRpt.Poller.Domain.Errors.Parser
{
    public class MalformedTagError : MalformItemError
    {
        private static readonly Guid _id = Guid.Parse("9854d2df-ad58-421f-91a1-e70978850b10");

        public MalformedTagError(string tagValue) 
            : base(_id, "tag", tagValue)
        {
        }
    }
}