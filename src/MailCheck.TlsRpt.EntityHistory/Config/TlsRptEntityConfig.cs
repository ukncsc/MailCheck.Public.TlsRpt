using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.TlsRpt.EntityHistory.Config
{
    public interface ITlsRptEntityHistoryConfig
    {
        string SnsTopicArn { get; }
    }

    public class TlsRptEntityHistoryConfig : ITlsRptEntityHistoryConfig
    {
        public TlsRptEntityHistoryConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string SnsTopicArn { get; }
    }
}
