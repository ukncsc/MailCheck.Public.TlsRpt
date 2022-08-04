using System.Collections.Generic;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.TlsRpt.Reports.Contracts;

namespace MailCheck.TlsRpt.Reports.Entity.Entity.Notifiers
{
    public class MessageEqualityComparer : IEqualityComparer<AdvisoryMessage>
    {
        public bool Equals(AdvisoryMessage x, AdvisoryMessage y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(AdvisoryMessage obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}