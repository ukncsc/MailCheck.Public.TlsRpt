using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.TlsRpt.Entity.Entity.Notifiers
{
    public interface IChangeNotifier
    {
        void Handle(TlsRptEntityState state, Message message);
    }
}
