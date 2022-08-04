using System;

namespace MailCheck.TlsRpt.Reports.Entity.Dao
{
    public class ProviderTotals
    {
        public DateTime ReportEndDate { get; set; }
        public string Provider { get; set; }
        public int TotalSuccessfulSessionCount { get; set; }
        public int TotalFailureSessionCount { get; set; }
    }
}