namespace MailCheck.TlsRpt.Contracts.SharedDomain
{
    public class UnknownUri : Uri
    {
        public UnknownUri(string value) 
            : base(nameof(UnknownUri), value)
        {
        }
    }
}