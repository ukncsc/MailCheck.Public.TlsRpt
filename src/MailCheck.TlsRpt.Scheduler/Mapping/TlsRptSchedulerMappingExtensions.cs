using MailCheck.TlsRpt.Contracts.Scheduler;
using MailCheck.TlsRpt.Scheduler.Dao.Model;

namespace MailCheck.TlsRpt.Scheduler.Mapping
{
    public static class TlsRptSchedulerMappingExtensions
    {
        public static TlsRptRecordExpired ToTlsRptRecordExpiredMessage(this TlsRptSchedulerState state) =>
            new TlsRptRecordExpired(state.Id);
    }
}
