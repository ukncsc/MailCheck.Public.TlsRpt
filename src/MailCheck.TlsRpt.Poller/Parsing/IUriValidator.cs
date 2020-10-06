namespace MailCheck.TlsRpt.Poller.Parsing
{
    public interface IUriValidator
    {
        bool IsValidUri(string uriToken);
    }
}