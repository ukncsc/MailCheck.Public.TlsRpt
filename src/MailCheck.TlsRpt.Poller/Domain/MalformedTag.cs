namespace MailCheck.TlsRpt.Poller.Domain
{
    public class MalformedTag : Tag
    {
        public MalformedTag(string rawValue)
            : base(nameof(MalformedTag), rawValue)
        {

        }
    }
}