using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using DnsClient;
using MailCheck.TlsRpt.Poller.Config;
using Microsoft.Extensions.DependencyInjection;

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
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new LookupClient(NameServer.GooglePublicDns, NameServer.GooglePublicDnsIPv6)
                {
                    Timeout = provider.GetRequiredService<ITlsRptPollerConfig>().DnsRecordLookupTimeout
                }
                : new LookupClient(new LookupClientOptions(provider.GetService<IDnsNameServerProvider>()
                    .GetNameServers()
                    .Select(_ => new IPEndPoint(_, 53)).ToArray())
                {
                    ContinueOnEmptyResponse = false,
                    UseCache = false,
                    UseTcpOnly = true,
                    EnableAuditTrail = true,
                    Timeout = provider.GetRequiredService<ITlsRptPollerConfig>().DnsRecordLookupTimeout
                });
        }
    }
}