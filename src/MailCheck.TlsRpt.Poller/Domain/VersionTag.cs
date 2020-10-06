namespace MailCheck.TlsRpt.Poller.Domain
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