using System;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using FluentValidation;
using FluentValidation.AspNetCore;
using MailCheck.Common.Api.Authentication;
using MailCheck.Common.Api.Authorisation.Service;
using MailCheck.Common.Api.Middleware;
using MailCheck.Common.Api.Middleware.Audit;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Sns;
using MailCheck.Common.SSM;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Reports.Api.Config;
using MailCheck.TlsRpt.Reports.Api.Controllers;
using MailCheck.TlsRpt.Reports.Api.Dao;
using MailCheck.TlsRpt.Reports.Api.Serialisation;
using MailCheck.TlsRpt.Reports.Api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.HealthChecks;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WebApiContrib.Core.Formatter.Csv;

namespace MailCheck.TlsRpt.Reports.Api
{
    public class StartUp
    {
        public StartUp(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            BsonSerializer.RegisterSerializationProvider(new EnumDescriptionSerializerProvider());

            if (RunInDevMode())
            {
                services.AddCors(CorsOptions);
            }

            services
                .AddLogging()
                .AddHealthChecks(checks =>
                    checks.AddValueTaskCheck("HTTP Endpoint", () =>
                        new ValueTask<IHealthCheckResult>(HealthCheckResult.Healthy("Ok"))))
                .AddTransient<IReportsApiDao, ReportsApiDao>()
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddTransient<IValidator<DomainDateRangeRequest>, DomainDateRangeRequestValidator>()
                .AddTransient<IDomainValidator, DomainValidator>()
                .AddTransient<IDocumentDbConfig, ApiConfig>()
                .AddTransient<IMessagePublisher, SnsMessagePublisher>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<IReportService, ReportService>()
                .AddTransient<IMongoClientProvider, MongoClientProvider>()
                .AddAudit("Tls-Rpt-Report-Api")
                .AddMailCheckAuthenticationClaimsPrincipleClient()
                .AddMvc(config =>
                {
                    AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    config.Filters.Add(new AuthorizeFilter(policy));
                    config.OutputFormatters.Add(new CsvOutputFormatter(new CsvFormatterOptions()
                    {
                        CsvDelimiter = ",",
                    }));
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                })
                .AddFluentValidation();

            services
                .AddAuthorization()
                .AddAuthentication(AuthenticationSchemes.Claims)
                .AddMailCheckClaimsAuthentication();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (RunInDevMode())
            {
                app.UseCors(CorsPolicyName);
            }

            app.UseMiddleware<AuditTimerMiddleware>()
               .UseMiddleware<OidcHeadersToClaimsMiddleware>()
               .UseMiddleware<ApiKeyToClaimsMiddleware>()
               .UseAuthentication()
               .UseMiddleware<AuditLoggingMiddleware>()
               .UseMiddleware<UnhandledExceptionMiddleware>()
               .UseMvc();
        }

        private bool RunInDevMode()
        {
            bool.TryParse(Environment.GetEnvironmentVariable("DevMode"), out bool isDevMode);
            return isDevMode;
        }

        private static Action<CorsOptions> CorsOptions => options =>
        {
            options.AddPolicy(CorsPolicyName, builder =>
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        };

        private const string CorsPolicyName = "CorsPolicy";
    }
}