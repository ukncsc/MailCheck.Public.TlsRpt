using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.TlsRpt.Entity.Config
{
    public interface ITlsRptEntityConfig
    {
        string SnsTopicArn { get; }
        string WebUrl { get; }
    }

    public class TlsRptEntityConfig : ITlsRptEntityConfig
    {
        public TlsRptEntityConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            WebUrl = environmentVariables.Get("WebUrl");
        }

        public string SnsTopicArn { get; }
        public string WebUrl { get; }
    }
}
