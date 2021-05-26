using System;
using Newtonsoft.Json;

namespace MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809
{
    public class DateRange: PermissiveSchema
    {
        [JsonProperty("start-datetime"), JsonRequired]
        public DateTime StartDatetime { get; set; }

        [JsonProperty("end-datetime"), JsonRequired]
        public DateTime EndDatetime { get; set; }
    }
}