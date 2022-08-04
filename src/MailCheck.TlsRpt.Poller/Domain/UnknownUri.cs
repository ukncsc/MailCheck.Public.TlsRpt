namespace MailCheck.TlsRpt.Poller.Domain
{
    public class UnknownUri : Uri
    {
        public UnknownUri(string value) 
            : base(nameof(UnknownUri), value)
        {
        }
    }
}