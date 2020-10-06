using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using MailCheck.Common.Messaging.Sqs;
using MailCheck.TlsRpt.Scheduler.StartUp;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace MailCheck.TlsRpt.Scheduler
{
    public class TlsRptSqsSchedulerLambdaEntryPoint : SqsTriggeredLambdaEntryPoint
    {
        public TlsRptSqsSchedulerLambdaEntryPoint()
            : base(new TlsRptSqsSchedulerLambdaStartUp()) { }
    }
}
