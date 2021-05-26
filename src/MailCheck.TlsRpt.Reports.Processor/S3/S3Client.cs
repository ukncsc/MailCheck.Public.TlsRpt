using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Reports.Processor.Domain;
using Microsoft.Extensions.Logging;
using S3Object = MailCheck.TlsRpt.Reports.Processor.Domain.S3Object;

namespace MailCheck.TlsRpt.Reports.Processor.S3
{
    public interface IS3Client
    {
        Task<S3Object> GetS3Object(S3SourceInfo s3SourceInfo);
    }

    internal class S3Client : IS3Client
    {
        private readonly IConfig _config;
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<S3Client> _log;

        public S3Client(IConfig config, IAmazonS3 s3Client, ILogger<S3Client> log)
        {
            _config = config;
            _s3Client = s3Client;
            _log = log;
        }

        public async Task<S3Object> GetS3Object(S3SourceInfo s3SourceInfo)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            using (GetObjectResponse response = await _s3Client
                .GetObjectAsync(s3SourceInfo.BucketName, s3SourceInfo.ObjectName)
                .TimeoutAfter(_config.TimeoutS3)
                .ConfigureAwait(false))
            {
                _log.LogDebug($"Retrieving aggregate report from bucket took {stopwatch.Elapsed}");

                stopwatch.Stop();
                
                using (MemoryStream memoryStream = new MemoryStream())
                using (Stream responseStream = response.ResponseStream)
                {
                    await responseStream.CopyToAsync(memoryStream);
                    return new S3Object(memoryStream.ToArray());
                }
            }
        }
    }
}