using System.Collections.Generic;

namespace MailCheck.TlsRpt.Contracts.SharedDomain
{
    public class RuaTag : Tag
    {
        public RuaTag(string rawValue, List<Uri> uris)
            : base(nameof(RuaTag), rawValue)
        {
            Uris = uris;
        }

        public List<Uri> Uris { get; }
    }
}