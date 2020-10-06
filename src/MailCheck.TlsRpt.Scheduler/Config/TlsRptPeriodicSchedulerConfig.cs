using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.TlsRpt.Scheduler.Config
{
    public interface ITlsRptPeriodicSchedulerConfig : ITlsRptSchedulerConfig
    {
        int DomainBatchSize { get; }
        long RefreshIntervalSeconds { get; }
    }

    public class TlsRptPeriodicSchedulerConfig : TlsRptSchedulerConfig, ITlsRptPeriodicSchedulerConfig
    {
        public TlsRptPeriodicSchedulerConfig(IEnvironmentVariables environmentVariables) : base(environmentVariables)
        {
            DomainBatchSize = environmentVariables.GetAsInt("DomainBatchSize");
            RefreshIntervalSeconds = environmentVariables.GetAsLong("RefreshIntervalSeconds");
        }

        public int DomainBatchSize { get; }

        public long RefreshIntervalSeconds { get; }
    }
}
