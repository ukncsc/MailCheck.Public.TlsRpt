using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MailCheck.TlsRpt.Reports.Contracts.Schema
{
    public class PermissiveSchema
    {
        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get;}

        public PermissiveSchema()
        {
            AdditionalData = new Dictionary<string, JToken>();
        }
    }
}