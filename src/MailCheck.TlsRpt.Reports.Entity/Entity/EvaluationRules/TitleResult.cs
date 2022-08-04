using System;

namespace MailCheck.TlsRpt.Reports.Entity.Entity.EvaluationRules
{
    public class TitleResult
    {
        public int Period { get; set; }
        public Tuple<DateTime, DateTime> DateRange { get; set; }
        public int ProviderCount { get; set; }
        public double PercentageFailures { get; set; }
    }
}