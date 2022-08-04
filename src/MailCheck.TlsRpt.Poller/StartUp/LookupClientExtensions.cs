using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using DnsClient;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Poller.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Poller.StartUp
{
    public static class LookupClientExtensions
    {
        public static IServiceCollection AddLookupClient(this IServiceCollection collection)
        {
            return collection.AddSingleton(CreateLookupClient);
        }

        private static ILookupClient CreateLookupClient(IServiceProvider provider)
        {
            LookupClient lookupClient = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new LookupClient(NameServer.GooglePublicDns, NameServer.GooglePublicDnsIPv6)
                {
                    Timeout = provider.GetRequiredService<ITlsRptPollerConfig>().DnsRecordLookupTimeout
                }
                : new LookupClient(new LookupClientOptions(provider.GetService<IDnsNameServerProvider>()
                    .GetNameServers()
                    .Select(_ => new IPEndPoint(_, 53)).ToArray())
                {
                    ContinueOnEmptyResponse = false,
                    ContinueOnDnsError = false,
                    UseCache = false,
                    Retries = 0,
                    UseTcpOnly = true,
                    EnableAuditTrail = true,
                    Timeout = provider.GetRequiredService<ITlsRptPollerConfig>().DnsRecordLookupTimeout
                });

            return new AuditTrailLoggingLookupClientWrapper(lookupClient, provider.GetService<IAuditTrailParser>(), provider.GetService<ILogger<AuditTrailLoggingLookupClientWrapper>>());
        }
    }
}