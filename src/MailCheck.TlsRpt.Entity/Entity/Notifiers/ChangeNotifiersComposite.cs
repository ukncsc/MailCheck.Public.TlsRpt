using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.TlsRpt.Entity.Entity.Notifiers
{
    public interface IChangeNotifiersComposite : IChangeNotifier
    {
    }

    public class ChangeNotifiersComposite : IChangeNotifiersComposite
    {
        private readonly IEnumerable<IChangeNotifier> _notifiers;

        public ChangeNotifiersComposite(IEnumerable<IChangeNotifier> notifiers)
        {
            _notifiers = notifiers;
        }

        public void Handle(TlsRptEntityState state, Message message)
        {
            foreach (IChangeNotifier changeNotifier in _notifiers)
            {
                changeNotifier.Handle(state, message);
            }
        }
    }
}