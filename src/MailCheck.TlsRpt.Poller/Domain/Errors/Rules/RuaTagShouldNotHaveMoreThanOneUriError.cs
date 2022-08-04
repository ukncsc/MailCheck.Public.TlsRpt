using System;

namespace MailCheck.TlsRpt.Poller.Domain.Errors.Rules
{
    public class RuaTagShouldNotHaveMoreThanOneUriError : Error
    {
        private static readonly Guid _Id = Guid.Parse("aba954bc-42ce-4cfd-ab37-ccd637c7d6d3");

        public RuaTagShouldNotHaveMoreThanOneUriError(int uriCount)
            : base(_Id, "mailcheck.tlsrpt.ruaTagShouldNotHaveMoreThanOneUri", ErrorType.Info, FormatError(uriCount), FormatMarkDown(uriCount))
        {
        }

        private static string FormatError(int uriCount) => string.Format(TlsRptRuleErrorMessages.RuaTagShouldNotHaveMoreThanOneUri, uriCount);
        private static string FormatMarkDown(int uriCount) => string.Format(TlsRptRuleMarkDown.RuaTagShouldNotHaveMoreThanOneUri, uriCount);
    }
}