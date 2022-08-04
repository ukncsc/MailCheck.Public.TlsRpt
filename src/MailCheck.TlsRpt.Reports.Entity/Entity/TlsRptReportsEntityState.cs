using System;
using System.Collections.Generic;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.TlsRpt.Reports.Contracts;

namespace MailCheck.TlsRpt.Reports.Entity
{
    public class TlsRptReportsEntityState 
    {
        public string Domain { get; set; }

        public int Version { get; set; } = 1;

        public DateTime Created { get; set; }

        public List<ReportsAdvisoryMessage> AdvisoryMessages { get; set; } = new List<ReportsAdvisoryMessage>();

        public DateTime? LastUpdated { get; set; }
    }
}
