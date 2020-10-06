using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MailCheck.TlsRpt.Contracts.SharedDomain.Deserialisation
{
    public class UriConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            string uriType = jo["type"].Value<string>();

            switch (uriType.ToLower())
            {
                case "malformeduri":
                    return JsonConvert.DeserializeObject<MalformedUri>(jo.ToString());
                case "httpsuri":
                    return JsonConvert.DeserializeObject<HttpsUri>(jo.ToString());
                case "unknownuri":
                    return JsonConvert.DeserializeObject<UnknownUri>(jo.ToString());
                case "mailtouri":
                    return JsonConvert.DeserializeObject<MailToUri>(jo.ToString());

                default:
                    throw new InvalidOperationException($"Failed to convert type of {uriType}.");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Uri);
        }
    }
}