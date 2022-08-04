using System;
using Newtonsoft.Json;

namespace MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809
{
    public class PermissiveStringArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsArray;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string[] result;

            try
            {
                // Use a new json serializer here to avoid the error handling specified by the JsonSerializerSettings attached
                // to the serializer passed in. We still want the error handling but we will handle it for this field.
                result = new JsonSerializer().Deserialize<string[]>(reader);
            }
            catch
            {
                result = null;
            }
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}