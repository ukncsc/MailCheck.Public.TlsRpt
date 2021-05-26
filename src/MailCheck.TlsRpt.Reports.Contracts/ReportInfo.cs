using MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809;
using Newtonsoft.Json;

namespace MailCheck.TlsRpt.Reports.Contracts
{
    public class ReportInfo
    {
        public JsonReport Report { get; set; }

        public string Version { get; set; }


        public string Source { get; set; }

        [JsonProperty("tls-report-domain")]
        public string TlsReportDomain { get; set; }

        [JsonProperty("tls-report-submitter")]
        public string TlsReportSubmitter { get; set; }

        public ReportInfo(JsonReport report, string version, string source, string tlsReportDomain, string tlsReportSubmitter)
        {
            Version = version;
            Report = report;
            Source = source;
            TlsReportDomain = tlsReportDomain;
            TlsReportSubmitter = tlsReportSubmitter;
        }
    }
}