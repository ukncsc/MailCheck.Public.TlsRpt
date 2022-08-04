using System.Collections.Generic;
using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Common.Environment.Implementations;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Notifiers;
using MailCheck.Common.SSM;
using MailCheck.TlsRpt.Contracts.SharedDomain.Deserialisation;
using MailCheck.TlsRpt.Entity.Config;
using MailCheck.TlsRpt.Entity.Dao;
using MailCheck.TlsRpt.Entity.Entity;
using MailCheck.TlsRpt.Entity.Entity.Notifiers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using FindingsChangedNotifier = MailCheck.Common.Processors.Notifiers.FindingsChangedNotifier;
using LocalFindingsChangedNotifier = MailCheck.TlsRpt.Entity.Entity.Notifiers.FindingsChangedNotifier;
using Message = MailCheck.TlsRpt.Contracts.SharedDomain.Message;
using MessageEqualityComparer = MailCheck.TlsRpt.Entity.Entity.Notifiers.MessageEqualityComparer;

namespace MailCheck.TlsRpt.Entity.StartUp
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
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>() 
                .AddTransient<ITlsRptEntityDao, TlsRptEntityDao>()
                .AddTransient<ITlsRptEntityConfig, TlsRptEntityConfig>()
                .AddTransient<IChangeNotifiersComposite, ChangeNotifiersComposite>()
                .AddTransient<IChangeNotifier, RecordChangedNotifier>()
                .AddTransient<IChangeNotifier, AdvisoryChangedNotifier>()
                .AddTransient<IChangeNotifier, LocalFindingsChangedNotifier>()
                .AddTransient<IFindingsChangedNotifier, FindingsChangedNotifier>()
                .AddTransient<IEqualityComparer<Message>, MessageEqualityComparer>()
                .AddTransient<TlsRptEntity>();
        }
    }
}