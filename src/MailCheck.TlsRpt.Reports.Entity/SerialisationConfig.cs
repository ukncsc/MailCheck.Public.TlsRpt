using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MailCheck.TlsRpt.Reports.Entity
{
    public static class SerialisationConfig
    {
        public static JsonSerializerSettings Settings => new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        };
    }
}