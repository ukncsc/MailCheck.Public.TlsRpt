using System.Linq;
using Newtonsoft.Json.Serialization;

namespace MailCheck.TlsRpt.Reports.Processor.Dao
{
    public class KebabToPascalNamingStrategy : NamingStrategy
    {
        public override string GetDictionaryKey(string key)
        {
            return Transform(key);
        }

        public override string GetExtensionDataName(string name)
        {
            return Transform(name);
        }

        public override string GetPropertyName(string name, bool hasSpecifiedName)
        {
            return Transform(name);
        }

        protected override string ResolvePropertyName(string name)
        {
            return Transform(name);
        }
        private static string Transform(string value)
        {
            return string.Join("", value.Split('-').Select(s => $"{s.Substring(0, 1).ToUpper()}{s.Substring(1).ToLower()}"));
        }
    }
}