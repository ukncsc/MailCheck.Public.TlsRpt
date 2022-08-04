using System.Collections.Generic;
using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Common.Environment.Implementations;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.Common.SSM;
using MailCheck.TlsRpt.Reports.Entity.Config;
using MailCheck.TlsRpt.Reports.Entity.Dao;
using MailCheck.TlsRpt.Reports.Entity.Entity;
using MailCheck.TlsRpt.Reports.Entity.Entity.Notifiers;
using MailCheck.TlsRpt.Reports.Entity.Entity.EvaluationRules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using MailCheck.Common.Util;
using MongoDB.Bson.Serialization;
using MailCheck.TlsRpt.Reports.Entity.Serialisation;
using MailCheck.Common.Processors.Notifiers;
using FindingsChangedNotifier = MailCheck.Common.Processors.Notifiers.FindingsChangedNotifier;
using LocalFindingsChangedNotifier = MailCheck.TlsRpt.Reports.Entity.Entity.Notifiers.FindingsChangedNotifier;
using MessageEqualityComparer = MailCheck.TlsRpt.Reports.Entity.Entity.Notifiers.MessageEqualityComparer;

namespace MailCheck.TlsRpt.Reports.Entity.StartUp
{
    internal class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () => SerialisationConfig.Settings;
            BsonSerializer.RegisterSerializationProvider(new EnumDescriptionSerializerProvider());

            services
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddTransient<IEnvironment, EnvironmentWrapper>()
                .AddTransient<IEnvironmentVariables, EnvironmentVariables>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>() 
                .AddTransient<ITlsRptReportsEntityDao, TlsRptReportsEntityDao>()
                .AddTransient<IDocumentDbConfig, DocumentDbConfig>()
                .AddTransient<IReportsDataDao, ReportsDataDao>()
                .AddTransient<IEvaluator<DomainProvidersResults>, Evaluator<DomainProvidersResults>>()
                .AddTransient<IRule<DomainProvidersResults>, TlsFailures>()
                .AddTransient<ITlsRptReportsEntityConfig, TlsRptReportsEntityConfig>()
                .AddTransient<IChangeNotifiersComposite, ChangeNotifiersComposite>()
                .AddTransient<IChangeNotifier, AdvisoryChangedNotifier>()
                .AddTransient<IChangeNotifier, LocalFindingsChangedNotifier>()
                .AddTransient<IFindingsChangedNotifier, FindingsChangedNotifier>()
                .AddTransient<IEqualityComparer<AdvisoryMessage>, MessageEqualityComparer>()
                .AddTransient<IClock, Clock>()
                .AddTransient<IReportPeriodCalculator, ReportPeriodCalculator>()
                .AddTransient<IMongoClientProvider, MongoClientProvider>()
                .AddTransient<TlsRptReportsEntity>();
        }
    }
}