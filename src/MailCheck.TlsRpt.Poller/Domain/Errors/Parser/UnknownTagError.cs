using System;

namespace MailCheck.TlsRpt.Poller.Domain.Errors.Parser
{
    public class UnknownTagError : UnknownItemError
    {
        private static readonly Guid _id = Guid.Parse("f5ab36d2-0bf0-4036-81c7-e69b25dd44c6");

        public UnknownTagError(string tagKey, string tagValue) 
            : base(_id, "tag", tagKey, tagValue)
        {
          
        }
    }
}