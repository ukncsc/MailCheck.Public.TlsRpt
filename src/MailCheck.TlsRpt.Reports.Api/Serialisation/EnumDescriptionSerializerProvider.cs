using System;
using System.Reflection;
using MongoDB.Bson.Serialization;

namespace MailCheck.TlsRpt.Reports.Api.Serialisation
{
    public class EnumDescriptionSerializerProvider : BsonSerializationProviderBase
    {
        public override IBsonSerializer GetSerializer(Type type, IBsonSerializerRegistry registry)
        {
            if (!type.GetTypeInfo().IsEnum) return null;

            Type enumSerializerType = typeof(EnumDescriptionSerializer<>).MakeGenericType(type);
            IBsonSerializer enumSerializer = (IBsonSerializer)Activator.CreateInstance(enumSerializerType);

            return enumSerializer;
        }
    }
}