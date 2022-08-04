using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using MailCheck.Common.Messaging.Sqs;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace MailCheck.TlsRpt.Reports.Entity
{
    public class LambdaEntryPoint : SqsTriggeredLambdaEntryPoint
    {
        public LambdaEntryPoint() : base(new StartUp.StartUp())
        {
        }
    }
}
