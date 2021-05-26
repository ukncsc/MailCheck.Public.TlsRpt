using System;

namespace MailCheck.TlsRpt.Reports.Api.Service
{
    public class ReportLine
    {
        public string Date { get; set; }
        public string OrganizationName { get; set; }
        public string PolicyType { get; set; }
        public string PolicyString { get; set; }
        public string PolicyDomain { get; set; }
        public string MxHost { get; set; }
        public string ResultType { get; set; }
        public string SendingMtaIp { get; set; }
        public string ReceivingMxHostname { get; set; }
        public string ReceivingMxHelo { get; set; }
        public string ReceivingIp { get; set; }
        public int FailedSessionCount { get; set; }
        public string AdditionalInformation { get; set; }
        public string FailureReasonCode { get; set; }
        public string TlsReportSubmitter { get; set; }
        public int TotalFailureSessionCount { get; set; }
        public int TotalSuccessfulSessionCount { get; set; }
    }
}