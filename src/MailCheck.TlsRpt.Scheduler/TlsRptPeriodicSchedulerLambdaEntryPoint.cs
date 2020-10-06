using MailCheck.Common.Messaging.CloudWatch;
using MailCheck.TlsRpt.Scheduler.StartUp;

namespace MailCheck.TlsRpt.Scheduler
{
    public class TlsRptPeriodicSchedulerLambdaEntryPoint : CloudWatchTriggeredLambdaEntryPoint
    {
        public TlsRptPeriodicSchedulerLambdaEntryPoint()
            : base(new TlsRptPeriodicSchedulerLambdaStartUp()) { }
    }
}