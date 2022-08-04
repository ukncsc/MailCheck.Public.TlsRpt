using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.TlsRpt.Reports.Entity.Config
{
    public interface ITlsRptReportsEntityConfig
    {
        string SnsTopicArn { get; }
        string WebUrl { get; }
    }

    public class TlsRptReportsEntityConfig : ITlsRptReportsEntityConfig
    {
        public TlsRptReportsEntityConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            WebUrl = environmentVariables.Get("WebUrl");
        }

        public string SnsTopicArn { get; }
        public string WebUrl { get; }
    }
}
