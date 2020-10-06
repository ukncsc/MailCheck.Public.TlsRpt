namespace MailCheck.TlsRpt.Contracts.SharedDomain
{
    public class VersionTag : Tag
    {
        public VersionTag(string rawValue, string value)
            : base(nameof(VersionTag), rawValue)
        {
            Value = value;
        }

        public string Value { get; }
    }
}