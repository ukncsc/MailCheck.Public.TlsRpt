using System;
using System.Collections.Generic;
using MailCheck.TlsRpt.Reports.Entity.Dao;

namespace MailCheck.TlsRpt.Reports.Entity.Entity
{
    public class DomainProvidersResults
    {
        public string Domain { get; set; }
        public List<ProviderTotals> ProviderTotals { get; set; }
        public Dictionary<int, List<ProviderFailure>> ProviderFailures { get; set; }
        public IDictionary<int, Tuple<DateTime, DateTime>> Periods { get; set; }
    }
}