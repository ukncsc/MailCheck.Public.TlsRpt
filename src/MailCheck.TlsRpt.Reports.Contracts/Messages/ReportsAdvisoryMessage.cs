using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.TlsRpt.Reports.Contracts
{
    public class ReportsAdvisoryMessage : AdvisoryMessage
    {
        public ReportsAdvisoryMessage(Guid id, string name, MessageType messageType, string text, string markDown, string period, MessageDisplay messageDisplay = MessageDisplay.Standard) : base(id, messageType, text, markDown, messageDisplay)
        {
            Period = period;
            Name = name;
        }

        public string Period { get; set; }
        public string Name { get; set; }
    }
}