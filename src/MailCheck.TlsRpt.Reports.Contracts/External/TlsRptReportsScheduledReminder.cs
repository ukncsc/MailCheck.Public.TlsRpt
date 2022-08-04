using MailCheck.Common.Contracts.Messaging;

namespace MailCheck.TlsRpt.Contracts.External
{
    public class TlsRptReportsScheduledReminder : ScheduledReminder
    {
        public TlsRptReportsScheduledReminder(string id, string resourceId)
             : base(id, resourceId)
        {
        }
    }
}
