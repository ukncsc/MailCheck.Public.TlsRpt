namespace MailCheck.TlsRpt.Contracts.SharedDomain
{
    public class HttpsUri : Uri{
        public HttpsUri(string value) 
            : base(nameof(HttpsUri),value)
        {
        }
    }
}