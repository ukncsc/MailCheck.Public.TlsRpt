namespace MailCheck.TlsRpt.Poller.Domain
{
    public class MailToUri : Uri
    {
        public MailToUri(string value) 
            : base(nameof(MailToUri),value)
        {
        }
    }
}