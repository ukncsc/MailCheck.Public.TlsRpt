using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MailCheck.TlsRpt.Reports.Api.Serialisation
{
    public class EnumDescriptionSerializer<TEnum> : StructSerializerBase<TEnum> where TEnum : struct
    {
        private static readonly Dictionary<string, TEnum> EnumValueLookup;
        private static readonly Dictionary<TEnum, string> EnumNameLookup;

        static EnumDescriptionSerializer()
        {
            EnumValueLookup = new Dictionary<string, TEnum>();
            EnumNameLookup = new Dictionary<TEnum, string>();

            Type enumType = typeof(TEnum);
            IEnumerable<FieldInfo> fields = enumType.GetRuntimeFields().Where(f => Enum.IsDefined(enumType, f.Name));

            foreach (FieldInfo field in fields)
            {
                TEnum fieldValue = (TEnum)field.GetValue(null);
                string fieldName = field.Name;
                string stringValue = fieldName;

                EnumValueLookup.Add(fieldName, fieldValue);

                if (field.GetCustomAttribute(typeof(EnumMemberAttribute), false) is EnumMemberAttribute jsonAttribute)
                {
                    string attributeValue = jsonAttribute.Value;
                    stringValue = attributeValue;
                    EnumValueLookup.Add(attributeValue, fieldValue);
                }

                EnumNameLookup.Add(fieldValue, stringValue);
            }
        }

        public override TEnum Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            string stringValue = context.Reader.ReadString();
            TEnum enumValue = GetEnumFromString(stringValue);
            return enumValue;
        }

        public static TEnum GetEnumFromString(string stringValue)
        {
            if (string.IsNullOrWhiteSpace(stringValue)) throw new ArgumentNullException(nameof(stringValue));

            if (EnumValueLookup.TryGetValue(stringValue, out TEnum val))
            {
                return val;
            }

            throw new Exception($"Failed to parse value {stringValue} into enum {typeof(TEnum)}");
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TEnum value)
        {
            if (EnumNameLookup.TryGetValue(value, out string name))
            {
                context.Writer.WriteString(name);
                return;
            }

            throw new Exception($"Failed to retrieve name for value {value} of enum {typeof(TEnum)}");
        }
    }
}
