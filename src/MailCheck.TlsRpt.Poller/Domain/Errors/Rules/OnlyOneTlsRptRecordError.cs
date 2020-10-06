using System;

namespace MailCheck.TlsRpt.Poller.Domain.Errors.Rules
{
    public class OnlyOneTlsRptRecordError : Error
    {
        private static readonly Guid _Id = Guid.Parse("a33972c3-d6b1-42d2-b75c-0d9030e2aacc");

        public OnlyOneTlsRptRecordError()
            : base(_Id, ErrorType.Error, TlsRptRuleErrorMessages.OnlyOneTlsRptRecordError, null)
        {
        }
    }
}