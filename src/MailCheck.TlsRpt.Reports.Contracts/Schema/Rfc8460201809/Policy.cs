using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809
{
    public class Policy : PermissiveSchema
    {
        [JsonProperty("policy-type"), JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        public PolicyType PolicyType { get; set; }

        [JsonProperty("policy-string")]
        public string[] PolicyString { get; set; }

        [JsonProperty("policy-domain"), JsonRequired]
        public string PolicyDomain { get; set; }

        [JsonProperty("mx-host")]
        public string[] MxHost { get; set; }
    }
}