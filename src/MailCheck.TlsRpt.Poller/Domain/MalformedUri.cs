namespace MailCheck.TlsRpt.Poller.Domain
{
    public class MalformedUri : Uri
    {
        public MalformedUri(string value) 
            : base(nameof(MalformedTag), value)
        {
        }
    }
}