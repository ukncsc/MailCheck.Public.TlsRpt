using System;

namespace MailCheck.TlsRpt.Poller.Domain.Errors.Parser
{
    public class UnknownItemError : Error
    {
        public UnknownItemError(Guid id, string name, string itemType, string itemKey, string itemValue, string markdown = null)
            : base(id, name, ErrorType.Error, FormatError(itemType, itemKey, itemValue), markdown)
        {
            
        }

        private static string FormatError(string itemType, string itemKey, string itemValue) => 
            string.Format(TlsRptParserErrorMessages.UnknownItemError, itemType, itemKey, itemValue);
    }
}