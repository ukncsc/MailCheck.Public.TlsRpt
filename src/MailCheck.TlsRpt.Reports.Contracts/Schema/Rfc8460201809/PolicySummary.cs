using System.Collections.Generic;
using Newtonsoft.Json;

namespace MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809
{
    public class PolicySummary : PermissiveSchema
    {
        [JsonProperty("policy"), JsonRequired]
        public Policy Policy { get; set; }

        [JsonProperty("summary"), JsonRequired]
        public Summary Summary { get; set; }

        [JsonProperty("failure-details")]
        public List<FailureDetail> FailureDetails { get; set; }
    }
}