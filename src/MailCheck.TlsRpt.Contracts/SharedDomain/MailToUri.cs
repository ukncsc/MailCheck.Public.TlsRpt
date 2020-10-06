namespace MailCheck.TlsRpt.Contracts.SharedDomain
{
    public class MailToUri : Uri
    {
        public MailToUri(string value) 
            : base(nameof(MailToUri),value)
        {
        }
    }
}