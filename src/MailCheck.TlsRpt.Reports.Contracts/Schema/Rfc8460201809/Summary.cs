using Newtonsoft.Json;

namespace MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809
{
    public class Summary : PermissiveSchema
    {
        [JsonProperty("total-successful-session-count"), JsonRequired]
        public int TotalSuccessfulSessionCount { get; set; }

        [JsonProperty("total-failure-session-count"), JsonRequired]
        public int TotalFailureSessionCount { get; set; }
    }
}