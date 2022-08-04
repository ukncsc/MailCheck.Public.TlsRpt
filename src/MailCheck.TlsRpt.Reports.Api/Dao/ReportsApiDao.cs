using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Reports.Api.Config;
using MailCheck.TlsRpt.Reports.Contracts;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;
using MailCheck.Common.Util;

namespace MailCheck.TlsRpt.Reports.Api.Dao
{
    public interface IReportsApiDao
    {
        Task<List<ProviderFailure>> GetFailures(string domain, int periodInDays, int limit);
        Task<List<ProviderTotals>> GetTotals(string domain, int periodInDays);
        Task<List<ReportPolicy>> GetDetails(string domain, int periodInDays);
    }

    public class ReportsApiDao : IReportsApiDao
    {
        private readonly IDocumentDbConfig _config;
        private readonly ILogger<ReportsApiDao> _logger;
        private readonly IClock _clock;
        private readonly IMongoClientProvider _mongoClientProvider;
        private const string CollectionName = "reports";

        public ReportsApiDao(IDocumentDbConfig config, ILogger<ReportsApiDao> logger, IClock clock, IMongoClientProvider mongoClientProvider)
        {
            _config = config;
            _logger = logger;
            _clock = clock;
            _mongoClientProvider = mongoClientProvider;
        }

        public async Task<List<ProviderFailure>> GetFailures(string domain, int periodInDays, int limit)
        {
            Stopwatch stopwatch = new Stopwatch();
            _logger.LogInformation($"Beginning {nameof(GetFailures)} for domain {domain} and periodInDays {periodInDays}");
            stopwatch.Start();

            IMongoCollection<ReportInfo> collection = await GetCollection();

            IMongoQueryable<ReportInfo> filteredReportInfos = collection.AsQueryable()
                .Where(x => x.TlsReportDomain == domain &&
                            x.Report.DateRange.EndDatetime > _clock.GetDateTimeUtc().Date.AddDays(periodInDays * -1) &&
                            x.Report.DateRange.EndDatetime < _clock.GetDateTimeUtc().Date);

            var providerConnections = filteredReportInfos.SelectMany(reportInfo => reportInfo.Report.Policies, (reportInfo, policySummary) => new
            {
                Provider = reportInfo.TlsReportSubmitter,
                policySummary.Summary.TotalSuccessfulSessionCount,
                policySummary.Summary.TotalFailureSessionCount,
                policySummary.FailureDetails
            });

            var providerFailedSessionCounts = providerConnections.SelectMany(x => x.FailureDetails, (parent, failureDetail) => new
            {
                parent.Provider,
                failureDetail.ResultType,
                failureDetail.FailedSessionCount
            });

            var failedSessionCountsByProviderAndType = providerFailedSessionCounts.GroupBy(x => new { x.Provider, x.ResultType });

            var failedSessionCountsSummed = failedSessionCountsByProviderAndType.Select(x => new
            {
                x.Key.Provider,
                x.Key.ResultType,
                FailedSessionCount = x.Sum(y => y.FailedSessionCount)
            });

            var failedSessionsFromDatabase = failedSessionCountsSummed
                .Take(limit)
                .ToList(); //Executes here

            var overallSessionsFromDatabase = providerConnections.GroupBy(x => x.Provider)
                .Select(x => new
                {
                    Provider = x.Key,
                    TotalSessions = x.Sum(y => y.TotalSuccessfulSessionCount) + x.Sum(y => y.TotalFailureSessionCount)
                })
                .ToList(); //Executes here

            _logger.LogInformation($"{nameof(GetFailures)} retrieved from database in {stopwatch.ElapsedMilliseconds}");

            IEnumerable<ProviderFailure> failedSessionPercentages = failedSessionsFromDatabase.Join(overallSessionsFromDatabase,
                outer => outer.Provider,
                inner => inner.Provider,
                (outer, inner) => new ProviderFailure
                {
                    Provider = outer.Provider,
                    ResultType = outer.ResultType,
                    Percent = outer.FailedSessionCount == 0 || inner.TotalSessions == 0 ? 0 : Math.Round((double)outer.FailedSessionCount / inner.TotalSessions * 100, 2)
                });

            _logger.LogInformation($"{nameof(GetFailures)} completed in {stopwatch.ElapsedMilliseconds}");

            stopwatch.Stop();

            return failedSessionPercentages.ToList();
        }

        public async Task<List<ProviderTotals>> GetTotals(string domain, int periodInDays)
        {
            Stopwatch stopwatch = new Stopwatch();
            _logger.LogInformation($"Beginning {nameof(GetTotals)} for domain {domain}");
            stopwatch.Start();

            IMongoCollection<ReportInfo> collection = await GetCollection();

            IMongoQueryable<ReportInfo> filteredReportInfos = collection.AsQueryable()
                .Where(x => x.TlsReportDomain == domain &&
                            x.Report.DateRange.EndDatetime > _clock.GetDateTimeUtc().Date.AddDays(-periodInDays) &&
                            x.Report.DateRange.EndDatetime < _clock.GetDateTimeUtc().Date);

            List<ProviderTotals> providerTotals = filteredReportInfos.SelectMany(info => info.Report.Policies,
                (reportInfo, policySummary) => new ProviderTotals
                {
                    ReportEndDate = reportInfo.Report.DateRange.EndDatetime,
                    Provider = reportInfo.TlsReportSubmitter,
                    TotalSuccessfulSessionCount = policySummary.Summary.TotalSuccessfulSessionCount,
                    TotalFailureSessionCount = policySummary.Summary.TotalFailureSessionCount
                })
                .ToList(); //Executes here

            _logger.LogInformation($"{nameof(GetTotals)} completed in {stopwatch.ElapsedMilliseconds}");
            stopwatch.Stop();

            return providerTotals;
        }

        public async Task<List<ReportPolicy>> GetDetails(string domain, int periodInDays)
        {
            Stopwatch stopwatch = new Stopwatch();
            _logger.LogInformation($"Beginning {nameof(GetDetails)} for domain {domain} and periodInDays {periodInDays}");
            stopwatch.Start();

            IMongoCollection<ReportInfo> collection = await GetCollection();

            IMongoQueryable<ReportInfo> filteredReportInfos = collection.AsQueryable()
                .Where(x => x.TlsReportDomain == domain &&
                            x.Report.DateRange.EndDatetime > _clock.GetDateTimeUtc().Date.AddDays(periodInDays * -1) &&
                            x.Report.DateRange.EndDatetime < _clock.GetDateTimeUtc().Date);

            IMongoQueryable<ReportPolicy> reportPolicySummaries = filteredReportInfos.SelectMany(reportInfo => reportInfo.Report.Policies, (reportInfo, policySummary) => new ReportPolicy
            {
                Date = reportInfo.Report.DateRange.StartDatetime,
                OrganizationName = reportInfo.Report.OrganizationName,
                PolicySummary = policySummary,
                TlsReportSubmitter = reportInfo.TlsReportSubmitter
            });

            List<ReportPolicy> result = reportPolicySummaries
                .ToList();  //Executes here

            _logger.LogInformation($"{nameof(GetDetails)} completed in {stopwatch.ElapsedMilliseconds}");
            stopwatch.Stop();

            return result;
        }

        private async Task<IMongoCollection<ReportInfo>> GetCollection()
        {
            IMongoClient client = await _mongoClientProvider.GetMongoClient();

            IMongoDatabase database = client.GetDatabase(_config.Database);
            IMongoCollection<ReportInfo> collection = database.GetCollection<ReportInfo>(CollectionName);

            return collection;
        }
    }
}