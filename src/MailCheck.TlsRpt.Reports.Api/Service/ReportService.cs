using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Reports.Api.Dao;

namespace MailCheck.TlsRpt.Reports.Api.Service
{
    public interface IReportService
    {
        Task<List<ProviderFailure>> GetSummaryBody(string requestDomain, int periodInDays);
        Task<List<TitleResult>> GetSummaryTitle(string requestDomain);
        Task<List<ReportLine>> GetSummaryDetails(string domain, int periodInDays);
    }

    public class ReportService : IReportService
    {
        private readonly IReportsApiDao _dao;
        private readonly IClock _clock;
        private const int MaxBodyRows = 30;

        public ReportService(IReportsApiDao dao, IClock clock)
        {
            _dao = dao;
            _clock = clock;
        }

        public async Task<List<TitleResult>> GetSummaryTitle(string requestDomain)
        {
            List<ProviderTotals> providerTotals = await _dao.GetTotals(requestDomain, 90);

            IEnumerable<TitleResult> titleResults = new List<int> { 2, 14, 90 }.Select(period =>
              {
                  DateTime periodStart = _clock.GetDateTimeUtc().Date.AddDays(-period);
                  List<ProviderTotals> periodTotals = providerTotals
                      .Where(y => y.ReportEndDate > periodStart).ToList();

                  if (periodTotals.Count == 0)
                  {
                      return null;
                  }

                  int totalFailures = periodTotals.Sum(y => y.TotalFailureSessionCount);
                  int totalSessions = periodTotals.Sum(y => y.TotalSuccessfulSessionCount) + totalFailures;

                  double percentageFailures;
                  if (totalSessions == 0 || totalFailures == 0)
                  {
                      percentageFailures = 0;
                  }
                  else
                  {
                      percentageFailures = Math.Round((double)totalFailures / totalSessions * 100, 2);
                  }

                  return new TitleResult
                  {
                      Period = period,
                      ProviderCount = periodTotals.Where(x => x.TotalFailureSessionCount > 0).GroupBy(y => y.Provider).Count(),
                  
                      PercentageFailures = percentageFailures
                  };
              });

            return titleResults.Where(x => x != null).ToList();
        }

        public async Task<List<ProviderFailure>> GetSummaryBody(string requestDomain, int periodInDays)
        {
            List<ProviderFailure> result = (await _dao.GetFailures(requestDomain, periodInDays, MaxBodyRows))
                .OrderBy(x => x.Provider)
                .ThenByDescending(x => x.Percent)
                .ToList();

            return result;
        }

        public async Task<List<ReportLine>> GetSummaryDetails(string domain, int periodInDays)
        {
            List<ReportPolicy> reportPolicySummaries = await _dao.GetDetails(domain, periodInDays);

            IEnumerable<ReportLine> reportsWithFailures = reportPolicySummaries
                .Where(x => x.PolicySummary.FailureDetails != null)
                .SelectMany(x => x.PolicySummary.FailureDetails, (summary, detail) =>
                {
                    ReportLine reportLine = new ReportLine
                    {
                        Date = summary.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                        OrganizationName = summary.OrganizationName,
                        TlsReportSubmitter = summary.TlsReportSubmitter,
                        AdditionalInformation = detail.AdditionalInformation,
                        FailedSessionCount = detail.FailedSessionCount,
                        FailureReasonCode = detail.FailureReasonCode,
                        ReceivingIp = detail.ReceivingIp,
                        ReceivingMxHelo = detail.ReceivingMxHelo,
                        ReceivingMxHostname = detail.ReceivingMxHostname,
                        ResultType = detail.ResultType.ToString(),
                        SendingMtaIp = detail.SendingMtaIp,
                        TotalFailureSessionCount = summary.PolicySummary.Summary.TotalFailureSessionCount,
                        TotalSuccessfulSessionCount = summary.PolicySummary.Summary.TotalSuccessfulSessionCount
                    };

                    if (summary.PolicySummary?.Policy != null)
                    {
                        reportLine.MxHost = summary.PolicySummary.Policy.MxHost != null
                            ? string.Join(", ", summary.PolicySummary.Policy.MxHost)
                            : null;
                        reportLine.PolicyDomain = summary.PolicySummary.Policy.PolicyDomain;
                        reportLine.PolicyString = summary.PolicySummary.Policy.PolicyString != null
                            ? string.Join(", ", summary.PolicySummary.Policy.PolicyString)
                            : null;
                        reportLine.PolicyType = summary.PolicySummary.Policy.PolicyType.ToString();
                    }

                    return reportLine;
                });

            IEnumerable<ReportLine> reportsWithoutFailures = reportPolicySummaries
                .Where(x => x.PolicySummary.FailureDetails == null)
                .Select(summary => new ReportLine
                {
                    Date = summary.Date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                    OrganizationName = summary.OrganizationName,
                    TlsReportSubmitter = summary.TlsReportSubmitter,

                    MxHost = summary.PolicySummary.Policy.MxHost != null ? string.Join(", ", summary.PolicySummary.Policy.MxHost) : null,
                    PolicyDomain = summary.PolicySummary.Policy.PolicyDomain,
                    PolicyString = summary.PolicySummary.Policy.PolicyString != null ? string.Join(", ", summary.PolicySummary.Policy.PolicyString) : null,
                    PolicyType = summary.PolicySummary.Policy.PolicyType.ToString(),

                    TotalFailureSessionCount = summary.PolicySummary.Summary.TotalFailureSessionCount,
                    TotalSuccessfulSessionCount = summary.PolicySummary.Summary.TotalSuccessfulSessionCount
                });

            List<ReportLine> results = reportsWithFailures
                .Concat(reportsWithoutFailures)
                .OrderBy(x => x.Date)
                .ThenBy(x => x.TlsReportSubmitter)
                .ThenBy(x => x.ResultType)
                .ToList();

            return results;
        }
    }
}
