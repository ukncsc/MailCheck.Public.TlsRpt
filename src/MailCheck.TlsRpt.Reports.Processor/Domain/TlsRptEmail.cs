using System.Collections.Generic;

namespace MailCheck.TlsRpt.Reports.Processor.Domain
{
    public class TlsRptEmail
    {
        public List<AttachmentInfo> Attachments { get; set; }
        public string TlsReportDomain { get; set; }
        public string TlsReportSubmitter { get; set; }
    }
}