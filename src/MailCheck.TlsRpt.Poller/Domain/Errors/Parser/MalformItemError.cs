using System;

namespace MailCheck.TlsRpt.Poller.Domain.Errors.Parser
{
    public class MalformItemError : Error
    {
        public MalformItemError(Guid id, string name, string itemType, string itemValue, string markdown = null) 
            : base(id, name, ErrorType.Error, FormatError(itemType, itemValue), markdown)
        {
        }

        private static string FormatError(string itemType, string itemValue) => string.Format(TlsRptParserErrorMessages.MalformedItemError, itemType, itemValue);
    }
}