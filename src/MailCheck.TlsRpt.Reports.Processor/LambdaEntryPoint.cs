using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MailCheck.TlsRpt.Reports.Processor
{
    public class LambdaEntryPoint
    {
        private readonly IReportProcessor _reportProcessor;

        public LambdaEntryPoint()
        {
            _reportProcessor = ReportProcessorFactory.Create();
        }

        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            await _reportProcessor.Process(evnt, context.AwsRequestId);
        }
    }
}