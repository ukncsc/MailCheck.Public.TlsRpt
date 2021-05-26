using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Common.Environment.Implementations;
using MailCheck.Common.Logging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Sns;
using MailCheck.Common.SSM;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Reports.Processor.Compression;
using MailCheck.TlsRpt.Reports.Processor.Dao;
using MailCheck.TlsRpt.Reports.Processor.Parser;
using MailCheck.TlsRpt.Reports.Processor.S3;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.TlsRpt.Reports.Processor
{
    internal static class ReportProcessorFactory
    {
        public static IReportProcessor Create()
        {
            return new ServiceCollection()
                .AddTransient<IReportProcessor, ReportProcessor>()
                .AddTransient<IS3SourceInfoFactory, S3SourceInfoFactory>()
                .AddTransient<IAmazonS3, AmazonS3Client>()
                .AddTransient<IS3Client, S3Client>()
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddTransient<IEnvironment, EnvironmentWrapper>()
                .AddTransient<IEnvironmentVariables, EnvironmentVariables>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<IConfig, Config>()
                .AddTransient<IDocumentDbConfig, Config>()
                .AddTransient<IMessagePublisher, SnsMessagePublisher>()
                .AddTransient<IMimeMessageFactory, MimeMessageFactory>()
                .AddTransient<IAttachmentStreamNormaliser, AttachmentStreamNormaliser>()
                .AddTransient<IContentTypeProvider, ContentTypeProvider>()
                .AddTransient<IGZipDecompressor, GZipDecompressor>()
                .AddTransient<IZipDecompressor, ZipDecompressor>()
                .AddTransient<IDomainValidator, DomainValidator>()
                .AddTransient<IReportParser, ReportParser>()
                .AddTransient<IAttachmentParser, AttachmentParser>()
                .AddTransient<IEmailBodyParser, EmailBodyParser>()
                .AddTransient<ITlsRptDao, TlsRptDao>()
                .AddSerilogLogging()
                .BuildServiceProvider()
                .GetRequiredService<IReportProcessor>();
        }
    }
}