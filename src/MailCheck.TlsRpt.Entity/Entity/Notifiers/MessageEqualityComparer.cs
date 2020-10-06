using System.Collections.Generic;
using MailCheck.TlsRpt.Contracts.SharedDomain;

namespace MailCheck.TlsRpt.Entity.Entity.Notifiers
{
    public class MessageEqualityComparer : IEqualityComparer<Message>
    {
        public bool Equals(Message x, Message y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(Message obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}