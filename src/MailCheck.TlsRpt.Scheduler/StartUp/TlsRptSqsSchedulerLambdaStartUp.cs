using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Data;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.Entity;
using MailCheck.TlsRpt.Scheduler.Config;
using MailCheck.TlsRpt.Scheduler.Dao;
using MailCheck.TlsRpt.Scheduler.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.TlsRpt.Scheduler.StartUp
{
    internal class TlsRptSqsSchedulerLambdaStartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            TlsRptSchedulerCommonStartUp.ConfigureCommonServices(services);

            services
                .AddTransient<ITlsRptSchedulerConfig, TlsRptSchedulerConfig>()
                .AddTransient<IHandle<TlsRptEntityCreated>, TlsRptSchedulerHandler>()
                .AddTransient<IHandle<DomainDeleted>, TlsRptSchedulerHandler>()
                .AddTransient<ITlsRptSchedulerDao, TlsRptSchedulerDao>()
                .AddSingleton<IDatabase, DefaultDatabase<MySqlProvider>>();
        }
    }
}
