using MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809;

namespace MailCheck.TlsRpt.Reports.Api.Dao
{
    public class ProviderFailure
    {
        public string Provider { get; set; }
        public ResultType ResultType { get; set; }
        public double Percent { get; set; }
    }
}