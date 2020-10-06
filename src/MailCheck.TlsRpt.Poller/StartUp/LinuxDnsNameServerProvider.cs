using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using MailCheck.TlsRpt.Poller.Config;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Poller.StartUp
{
    public interface IDnsNameServerProvider
    {
        List<IPAddress> GetNameServers();
    }

    public class LinuxDnsNameServerProvider : IDnsNameServerProvider
    {
        private readonly ILogger<LinuxDnsNameServerProvider> _log;
        private readonly ITlsRptPollerConfig _config;
        private const string NameServerFileName = "/etc/resolv.conf";
        private static readonly Regex Regex = new Regex(@"(?<=(?:^nameserver\s))(.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromSeconds(1));
        private List<IPAddress> _nameServers;

        public LinuxDnsNameServerProvider(ILogger<LinuxDnsNameServerProvider> log, ITlsRptPollerConfig config)
        {
            _log = log;
            _config = config;
        }

        public List<IPAddress> GetNameServers()
        {
            if (!string.IsNullOrWhiteSpace(_config.NameServer))
            {
                string[] ips = _config.NameServer.Split(',');
                return ips.Select(IPAddress.Parse).ToList();
            }

            if (_nameServers != null)
            {
                return _nameServers;
            }

            string nameServerFileContents = File.ReadAllText(NameServerFileName);
            MatchCollection matches = Regex.Matches(nameServerFileContents);

            List<string> ipStrings = (from Match match in matches select match.Value).Distinct().ToList();

            List<IPAddress> ipAddresses = new List<IPAddress>();

            foreach (var ipString in ipStrings)
            {
                if (IPAddress.TryParse(ipString, out IPAddress ipAddress))
                {
                    ipAddresses.Add(ipAddress);
                }
                else
                {
                    _log.LogWarning("Failed to parse {IP} to IPAddress", ipString);
                }
            }

            _nameServers = ipAddresses;

            return ipAddresses;
        }
    }
}
