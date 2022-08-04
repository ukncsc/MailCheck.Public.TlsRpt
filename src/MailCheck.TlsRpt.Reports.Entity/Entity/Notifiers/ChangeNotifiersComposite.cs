using System.Collections.Generic;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.TlsRpt.Reports.Contracts;

namespace MailCheck.TlsRpt.Reports.Entity.Entity.Notifiers
{
    public interface IChangeNotifiersComposite
    {
        void Handle(string domain, List<ReportsAdvisoryMessage> advisoryMessages1, List<ReportsAdvisoryMessage> advisoryMessages2);
    }

    public class ChangeNotifiersComposite : IChangeNotifiersComposite
    {
        private readonly IEnumerable<IChangeNotifier> _notifiers;

        public ChangeNotifiersComposite(IEnumerable<IChangeNotifier> notifiers)
        {
            _notifiers = notifiers;
        }

        public void Handle(string domain, List<ReportsAdvisoryMessage> current, List<ReportsAdvisoryMessage> incoming)
        {
            foreach (IChangeNotifier changeNotifier in _notifiers)
            {
                changeNotifier.Handle(domain, current, incoming);
            }
        }
    }
}