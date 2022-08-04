using System;

namespace MailCheck.TlsRpt.Poller.Domain.Errors.Rules
{
    public class NoTlsRptError : Error
    {
        private static readonly Guid _Id = Guid.Parse("8ec28d23-67b4-4e62-aca4-094eae3ebeae");

        public NoTlsRptError(string domain)
            : base(_Id, "mailcheck.tlsrpt.noTlsRpt", ErrorType.Warning, TlsRptRuleErrorMessages.NoTlsRptError, FormatMarkDown(domain))
        {
        }

        private static string FormatMarkDown(string domain) => string.Format(TlsRptRuleMarkDown.NoTlsRptError, domain);
    }
}
