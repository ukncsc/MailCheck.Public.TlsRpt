using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809
{
    public class FailureDetail: PermissiveSchema
    {
        [JsonProperty("result-type"), JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResultType ResultType { get; set; }

        [JsonProperty("sending-mta-ip")]
        public string SendingMtaIp { get; set; }

        [JsonProperty("receiving-mx-hostname")]
        public string ReceivingMxHostname { get; set; }

        [JsonProperty("receiving-mx-helo")]
        public string ReceivingMxHelo { get; set; }

        [JsonProperty("receiving-ip")]
        public string ReceivingIp { get; set; }

        [JsonProperty("failed-session-count"), JsonRequired]
        public int FailedSessionCount { get; set; }

        [JsonProperty("additional-information")]
        public string AdditionalInformation { get; set; }

        [JsonProperty("failure-reason-code")]
        public string FailureReasonCode { get; set; }
    }
}