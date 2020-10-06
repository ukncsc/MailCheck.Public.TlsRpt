using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.TlsRpt.Evaluator.Config
{
    public interface ITlsRptEvaluatorConfig
    {
        string SnsTopicArn { get; }
    }

    public class TlsRptEvaluatorConfig : ITlsRptEvaluatorConfig
    {
        public TlsRptEvaluatorConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string SnsTopicArn { get; }
    }
}
