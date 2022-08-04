using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Reports.Contracts;
using MailCheck.TlsRpt.Reports.Api.Dao;
using MailCheck.TlsRpt.Reports.Api.Domain;

namespace MailCheck.TlsRpt.Reports.Api.Service
{
    public interface ITlsRptReportsService
    {
        Task<List<ReportLine>> GetSummaryDetails(string domain, int periodInDays);
        Task<TlsRptReportsEntityResponse> GetTlsRptReportsEntity(string domain);
    }

    public class TlsRptReportsService : ITlsRptReportsService
    {
        private readonly IReportsApiDao _reportsDao;
        private readonly IReportsApiEntitiesDao _entitiesDao;

        public TlsRptReportsService(IReportsApiDao reportsDao, IReportsApiEntitiesDao entitiesDao)
        {
            _reportsDao = reportsDao;
            _entitiesDao = entitiesDao;
        }

        public async Task<TlsRptReportsEntityResponse> GetTlsRptReportsEntity(string domain)
        {
            return await _entitiesDao.Read(domain); ;
        }

        public async Task<List<ReportLine>> GetSummaryDetails(string domain, int periodInDays)
        {
            List<ReportPolicy> reportPolicySummaries = await _reportsDao.GetDetails(domain, periodInDays);

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
