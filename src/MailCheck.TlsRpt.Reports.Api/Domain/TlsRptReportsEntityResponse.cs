using System;
using System.Collections.Generic;
using MailCheck.TlsRpt.Reports.Contracts;

namespace MailCheck.TlsRpt.Reports.Api.Domain
{
    public class TlsRptReportsEntityResponse
    {
        public string Domain { get; set; }

        public List<ReportsAdvisoryMessage> AdvisoryMessages { get; set; } = null;

        public DateTime? LastUpdated { get; set; } = null;
    }
}