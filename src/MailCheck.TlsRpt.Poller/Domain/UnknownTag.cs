namespace MailCheck.TlsRpt.Poller.Domain
{
    public class UnknownTag : Tag
    {
        public UnknownTag(string rawValue)
            : base(nameof(UnknownTag),rawValue)
        {
            
        }
    }
}