using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using MailCheck.TlsRpt.Reports.Contracts;
using MailCheck.TlsRpt.Reports.Processor.Dao;
using MailCheck.TlsRpt.Reports.Processor.Domain;
using MailCheck.TlsRpt.Reports.Processor.Parser;
using MailCheck.TlsRpt.Reports.Processor.S3;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Reports.Processor
{
    public interface IReportProcessor
    {
        Task Process(SQSEvent sqsEvent, string requestId);
    }

    public class ReportProcessor : IReportProcessor
    {
        private readonly IS3Client _client;
        private readonly IConfig _config;
        private readonly ITlsRptDao _dao;
        private readonly ILogger<ReportProcessor> _logger;
        private readonly IReportParser _reportParser;
        private readonly IS3SourceInfoFactory _s3SourceInfoFactory;
        private readonly IEmailBodyParser _emailBodyParser;

        public ReportProcessor(ILogger<ReportProcessor> logger, IS3Client client, IConfig config, IReportParser reportParser, ITlsRptDao dao, IS3SourceInfoFactory s3SourceInfoFactory, IEmailBodyParser emailBodyParser)
        {
            _logger = logger;
            _client = client;
            _config = config;
            _reportParser = reportParser;
            _dao = dao;
            _s3SourceInfoFactory = s3SourceInfoFactory;
            _emailBodyParser = emailBodyParser;
        }

        public async Task Process(SQSEvent sqsEvent, string requestId)
        {
            List<S3SourceInfo> s3SourceInfos = _s3SourceInfoFactory.Create(sqsEvent, requestId);
            IEnumerable<Task> processTasks = s3SourceInfos.Select(Process);

            await Task.WhenAll(processTasks);
        }

        private async Task Process(S3SourceInfo s3SourceInfo)
        {
            string uri = $"{s3SourceInfo.BucketName}/{s3SourceInfo.ObjectName}";
            string identifier = $"message Id: {s3SourceInfo.MessageId}, request Id: {s3SourceInfo.RequestId}.";
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["MessageId"] = s3SourceInfo.MessageId,
                ["RequestId"] = s3SourceInfo.RequestId,
                ["S3ObjectPath"] = uri
            }))
            {
                _logger.LogDebug($"Processing report in s3 object {uri}, " + identifier);

                try
                {
                    S3Object s3Object = await _client.GetS3Object(s3SourceInfo);

                    _logger.LogDebug($"Successfully retrieved report in s3 object {uri}, " + identifier);

                    if (s3Object.Content.Length / 1024 > _config.MaxS3ObjectSizeKilobytes)
                    {
                        _logger.LogWarning(
                            $"Didnt process report in s3 object {uri} " +
                            $" as MaxS3ObjectSizeKilobytes of {_config.MaxS3ObjectSizeKilobytes} Kb was exceeded, " + identifier);
                        return;
                    }

                    TlsRptEmail tlsRptEmail = _emailBodyParser.Parse(s3Object.Content);

                    ReportInfo reportInfo = _reportParser.Parse(tlsRptEmail, uri);

                    if (reportInfo.Report != null)
                    {
                        _logger.LogDebug($"Successfully parsed report in s3 object {uri}, " + identifier);

                        await _dao.Persist(reportInfo);

                        _logger.LogDebug($"Successfully persisted report in s3 object {uri}, " + identifier);
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to parse report in s3 object {uri}, " + identifier);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to process s3 object {s3SourceInfo.BucketName}/{s3SourceInfo.ObjectName} message Id: {s3SourceInfo.MessageId}, request Id: {s3SourceInfo.RequestId}");
                    throw;
                }
            }
        }
    }
}