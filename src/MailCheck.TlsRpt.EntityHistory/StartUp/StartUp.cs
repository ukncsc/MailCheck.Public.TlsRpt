using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Common.Environment.Implementations;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.SharedDomain.Deserialisation;
using MailCheck.TlsRpt.EntityHistory.Config;
using MailCheck.TlsRpt.EntityHistory.Dao;
using MailCheck.TlsRpt.EntityHistory.Entity;
using MailCheck.TlsRpt.EntityHistory.Service;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MailCheck.TlsRpt.EntityHistory.StartUp
{
    internal class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () => SerialisationConfig.Settings;

            services
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddTransient<IEnvironment, EnvironmentWrapper>()
                .AddTransient<IEnvironmentVariables, EnvironmentVariables>()
                .AddTransient<ITlsRptEntityHistoryConfig, TlsRptEntityHistoryConfig>()
                .AddTransient<ITlsRptRuaService, TlsRptRuaService>()
                .AddTransient<ITlsRptRuaValidator, TlsRptRuaValidator>()
                .AddTransient<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<ITlsRptEntityHistoryDao, TlsRptEntityHistoryDao>()
                .AddTransient<TlsRptEntityHistory>();
        }
    }
}
