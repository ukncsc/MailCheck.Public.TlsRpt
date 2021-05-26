using System.Collections.Generic;
using Newtonsoft.Json;

namespace MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809
{
    public class JsonReport : PermissiveSchema
    {
        public static string Version => "RFC-8460_2018-09";

        [JsonProperty("organization-name"), JsonRequired]
        public string OrganizationName { get; set; }

        [JsonProperty("date-range"), JsonRequired]
        public DateRange DateRange { get; set; }

        [JsonProperty("contact-info")]
        public string ContactInfo { get; set; }

        [JsonProperty("report-id"), JsonRequired]
        public string ReportId { get; set; }

        [JsonProperty("policies"), JsonRequired]
        public List<PolicySummary> Policies { get; set; }
    }
}