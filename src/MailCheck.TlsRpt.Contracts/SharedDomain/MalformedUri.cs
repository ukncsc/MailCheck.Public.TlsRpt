namespace MailCheck.TlsRpt.Contracts.SharedDomain
{
    public class MalformedUri : Uri
    {
        public MalformedUri(string value) 
            : base(nameof(MalformedUri), value)
        {
        }
    }
}