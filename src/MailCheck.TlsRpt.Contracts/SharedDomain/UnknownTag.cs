namespace MailCheck.TlsRpt.Contracts.SharedDomain
{
    public class UnknownTag : Tag
    {
        public UnknownTag(string rawValue)
            : base(nameof(UnknownTag),rawValue)
        {
            
        }
    }
}