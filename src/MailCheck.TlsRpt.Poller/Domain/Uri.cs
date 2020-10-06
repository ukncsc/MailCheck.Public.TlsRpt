namespace MailCheck.TlsRpt.Poller.Domain
{
    public abstract class Uri 
    {
        protected Uri(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; }
        public string Value { get; }
    }
}