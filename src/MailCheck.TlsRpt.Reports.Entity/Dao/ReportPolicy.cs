using System;
using MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809;

namespace MailCheck.TlsRpt.Reports.Entity.Dao
{
    public class ReportPolicy
    {
        public DateTime Date { get; set; }
        public string OrganizationName { get; set; }
        public string TlsReportSubmitter { get; set; }
        public PolicySummary PolicySummary { get; set; }
    }
}