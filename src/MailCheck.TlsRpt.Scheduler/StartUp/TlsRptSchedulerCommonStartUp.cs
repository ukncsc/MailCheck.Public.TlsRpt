using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.SSM;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MailCheck.TlsRpt.Scheduler.StartUp
{
    public static class TlsRptSchedulerCommonStartUp
    {
        public static void ConfigureCommonServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };

            services
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>();
        }
    }
}
