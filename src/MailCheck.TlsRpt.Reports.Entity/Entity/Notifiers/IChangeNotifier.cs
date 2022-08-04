using System.Collections.Generic;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.TlsRpt.Reports.Contracts;

namespace MailCheck.TlsRpt.Reports.Entity.Entity.Notifiers
{
    public interface IChangeNotifier
    {
        void Handle(string domain, List<ReportsAdvisoryMessage> current, List<ReportsAdvisoryMessage> incoming);
    }
}
