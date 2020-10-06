using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.TlsRpt.Api.Config
{
    public interface ITlsRptApiConfig
    {
        string MicroserviceOutputSnsTopicArn { get; }
        string SnsTopicArn { get; }
    }

    public class TlsRptApiConfig : ITlsRptApiConfig
    {
        public TlsRptApiConfig(IEnvironmentVariables environmentVariables)
        {
            MicroserviceOutputSnsTopicArn = environmentVariables.Get("MicroserviceOutputSnsTopicArn");
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string MicroserviceOutputSnsTopicArn { get; }
        public string SnsTopicArn { get; }
    }
}
