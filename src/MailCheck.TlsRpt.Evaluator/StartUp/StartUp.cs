using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Logging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.SSM;
using MailCheck.TlsRpt.Contracts.Poller;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using MailCheck.TlsRpt.Contracts.SharedDomain.Deserialisation;
using MailCheck.TlsRpt.Evaluator.Config;
using MailCheck.TlsRpt.Evaluator.Explainers;
using MailCheck.TlsRpt.Evaluator.Rules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MailCheck.TlsRpt.Evaluator.StartUp
{
    internal class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () => SerialisationConfig.Settings;

            services
                .AddTransient<IHandle<TlsRptRecordsPolled>, EvaluationHandler>()
                .AddTransient<ITlsRptEvaluationProcessor, TlsRptEvaluationProcessor>()
                .AddTransient<ITagExplainer, VersionTagExplainer>()
                .AddTransient<ITagExplainer, RuaTagExplainer>()
                .AddTransient<ITlsRptRecordExplainer, TlsRptRecordExplainer>()
                .AddTransient<ITlsRptRecordsExplainer, TlsRptRecordsExplainer>()

                .AddTransient<IEvaluator<TlsRptRecord>, Evaluator<TlsRptRecord>>()
                .AddTransient<IRule<TlsRptRecord>, RuaTagsShouldContainTlsRptServiceMailBox>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<ITlsRptEvaluatorConfig, TlsRptEvaluatorConfig>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddSerilogLogging();
        }
    }
}
