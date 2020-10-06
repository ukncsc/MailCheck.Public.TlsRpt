using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.TlsRpt.Scheduler.Config
{
    public interface ITlsRptSchedulerConfig
    {
        string PublisherConnectionString { get; }
    }

    public class TlsRptSchedulerConfig : ITlsRptSchedulerConfig
    {
        public TlsRptSchedulerConfig(IEnvironmentVariables environmentVariables)
        {
            PublisherConnectionString = environmentVariables.Get("SnsTopicArn");
        }

        public string PublisherConnectionString { get; }
    }
}
