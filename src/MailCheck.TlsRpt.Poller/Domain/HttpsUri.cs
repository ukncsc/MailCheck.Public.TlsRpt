namespace MailCheck.TlsRpt.Poller.Domain
{
    public class HttpsUri : Uri{
        public HttpsUri(string value) 
            : base(nameof(HttpsUri),value)
        {
        }
    }
}