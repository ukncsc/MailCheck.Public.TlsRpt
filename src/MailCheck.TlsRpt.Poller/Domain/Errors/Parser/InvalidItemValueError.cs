using System;

namespace MailCheck.TlsRpt.Poller.Domain.Errors.Parser
{
    public class InvalidItemValueError : Error
    {
        public InvalidItemValueError(Guid id, string itemKey, string itemValue)
            : base(id, ErrorType.Error, FormatString(itemKey, itemValue), null)
        {
        }

        private static string FormatString(string tagKey, string tagValue) =>
            string.Format(TlsRptParserErrorMessages.InvalidItemValueError, tagKey, tagValue);
    }
}