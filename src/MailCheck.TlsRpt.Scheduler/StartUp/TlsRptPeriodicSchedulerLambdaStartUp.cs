using MailCheck.Common.Data;
using MailCheck.Common.Environment.FeatureManagement;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Sns;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Scheduler.Config;
using MailCheck.TlsRpt.Scheduler.Dao;
using MailCheck.TlsRpt.Scheduler.Processor;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.TlsRpt.Scheduler.StartUp
{
    internal class TlsRptPeriodicSchedulerLambdaStartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            TlsRptSchedulerCommonStartUp.ConfigureCommonServices(services);


            services
                .AddTransient<ITlsRptPeriodicSchedulerConfig, TlsRptPeriodicSchedulerConfig>()
                .AddTransient<IProcess, TlsRptPollSchedulerProcessor>()
                .AddTransient<IMessagePublisher, SnsMessagePublisher>()
                .AddSingleton<IDatabase, DefaultDatabase<MySqlProvider>>()
                .AddTransient<IClock, Clock>()
                .AddTransient<ITlsRptPeriodicSchedulerDao, TlsRptPeriodicSchedulerDao>();
        }
    }
}
