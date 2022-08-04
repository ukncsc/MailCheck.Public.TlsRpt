using Amazon.SimpleNotificationService;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Common.Environment.Implementations;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Poller.Config;
using MailCheck.TlsRpt.Poller.Dns;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Parsing;
using MailCheck.TlsRpt.Poller.Rules;
using MailCheck.TlsRpt.Poller.Rules.Record;
using MailCheck.TlsRpt.Poller.Rules.Records;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.TlsRpt.Poller.StartUp
{
    internal class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings serializerSetting = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };

                serializerSetting.Converters.Add(new StringEnumConverter());

                return serializerSetting;
            };

            services
                .AddSingleton<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<PollHandler>()
                .AddTransient<ITlsRptProcessor, TlsRptProcessor>()
                .AddTransient<IDnsClient, Dns.DnsClient>()
                .AddTransient<ITlsRptPollerConfig, TlsRptPollerConfig>()
                .AddTransient<IAuditTrailParser, AuditTrailParser>()
                .AddSingleton<IDnsNameServerProvider, LinuxDnsNameServerProvider>()
                .AddTransient<IEnvironmentVariables, EnvironmentVariables>()
                .AddTransient<IEnvironment, EnvironmentWrapper>()
                .AddTransient<ITlsRptRecordsParser, TlsRptRecordsParser>()
                .AddTransient<ITlsRptRecordParser, TlsRptRecordParser>()
                .AddTransient<ITagParser, VersionParser>()
                .AddTransient<ITagParser, RuaParser>()
                .AddTransient<IUriParser, MailToRuaParser>()
                .AddTransient<IUriParser, HttpsRuaParser>()
                .AddTransient<IHttpsUriValidator, HttpsUriValidator>()
                .AddTransient<IMailToUriValidator, MailToUriValidator>()
                .AddTransient<ITlsRptRecordsEvaluator, TlsRptRecordsEvaluator>()
                .AddTransient<IEvaluator<TlsRptRecords>, Evaluator<TlsRptRecords>>()
                .AddTransient<IEvaluator<TlsRptRecord>, Evaluator<TlsRptRecord>>()
                .AddTransient<IRule<TlsRptRecords>, NoTlsRptRecord>()
                .AddTransient<IRule<TlsRptRecords>, OnlyOneTlsRptRecord>()
                .AddTransient<IRule<TlsRptRecord>, RuaTagShouldNotHaveMoreThanOneUri>()
                .AddLookupClient();
        }        
    }
}
