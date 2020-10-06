namespace MailCheck.TlsRpt.Poller.Domain
{
    public abstract class Tag
    {
        protected Tag(string type, string rawValue)
        {
            Type = type;
            RawValue = rawValue;
        }

        public string Type { get; }
        public string RawValue { get; }
    }
}