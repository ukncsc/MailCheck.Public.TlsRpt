using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MailCheck.TlsRpt.Poller.Domain.Deserialisation
{
    public class TagConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            string tagType = jo["type"].Value<string>();

            switch (tagType.ToLower())
            {
                case "malformedtag":
                    return JsonConvert.DeserializeObject<MalformedTag>(jo.ToString());
                case "ruatag":
                    return JsonConvert.DeserializeObject<RuaTag>(jo.ToString(), SerialisationConfig.Settings);
                case "unknowntag":
                    return JsonConvert.DeserializeObject<UnknownTag>(jo.ToString());
                case "versiontag":
                    return JsonConvert.DeserializeObject<VersionTag>(jo.ToString());
                
                default:
                    throw new InvalidOperationException($"Failed to convert type of {tagType}.");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Tag);
        }
    }
}