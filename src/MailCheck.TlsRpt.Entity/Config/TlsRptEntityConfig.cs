using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.TlsRpt.Entity.Config
{
    public interface ITlsRptEntityConfig
    {
        string SnsTopicArn { get; }
    }

    public class TlsRptEntityConfig : ITlsRptEntityConfig
    {
        public TlsRptEntityConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string SnsTopicArn { get; }
    }
}
