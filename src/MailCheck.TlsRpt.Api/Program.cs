using System;
using MailCheck.Common.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MailCheck.TlsRpt.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            IWebHostBuilder webHostBuilder = WebHost.CreateDefaultBuilder(args)
                .UseStartup<StartUp>()
                .UseHealthChecks("/healthcheck");

            if (RunInDevMode())
            {
                webHostBuilder.UseUrls("http://localhost:5007");
            }

            return webHostBuilder;
        }

        private static bool RunInDevMode()
        {
            bool.TryParse(Environment.GetEnvironmentVariable("DevMode"), out bool isDevMode);
            return isDevMode;
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLogging(this IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(new RenderedJsonFormatter())
                .CreateLogger();

            return services
                .AddLogging(_ => _.AddSerilog());
        }
    }
}
