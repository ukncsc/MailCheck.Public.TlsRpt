namespace MailCheck.TlsRpt.Contracts.SharedDomain
{
    public class MalformedTag : Tag
    {
        public MalformedTag(string rawValue)
            : base(nameof(MalformedTag), rawValue)
        {

        }
    }
}