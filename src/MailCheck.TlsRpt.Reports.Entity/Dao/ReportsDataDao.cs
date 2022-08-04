using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Reports.Contracts;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Reports.Entity.Config;

namespace MailCheck.TlsRpt.Reports.Entity.Dao
{
    public interface IReportsDataDao
    {
        Task<Dictionary<int, List<ProviderFailure>>> GetFailures(string domain, int limit, IDictionary<int, Tuple<DateTime, DateTime>> periodDateRanges);
        Task<List<ProviderTotals>> GetTotals(string domain, Tuple<DateTime, DateTime> periodDateRange);
    }

    public class ReportsDataDao : IReportsDataDao
    {
        private readonly IDocumentDbConfig _config;
        private readonly ILogger<ReportsDataDao> _logger;
        private readonly IMongoClientProvider _mongoClientProvider;
        
        private const string CollectionName = "reports";

        public ReportsDataDao(IDocumentDbConfig config, ILogger<ReportsDataDao> logger, IMongoClientProvider mongoClientProvider)
        {
            _config = config;
            _logger = logger;
            _mongoClientProvider = mongoClientProvider;
        }

        public async Task<Dictionary<int, List<ProviderFailure>>> GetFailures(string domain, int limit, IDictionary<int, Tuple<DateTime, DateTime>> periodDateRanges)
        {
            Stopwatch stopwatch = new Stopwatch();
            _logger.LogInformation($"Beginning {nameof(GetFailures)} for domain {domain}");
            stopwatch.Start();

            IMongoCollection<ReportInfo> collection = await GetCollection();

            Dictionary<int, List<ProviderFailure>> titleResults = periodDateRanges.Keys.ToDictionary<int, int, List<ProviderFailure>>(p => p, p => null);

            foreach (KeyValuePair<int, Tuple<DateTime, DateTime>> period in periodDateRanges)
            {
                IMongoQueryable<ReportInfo> filteredReportInfos = QueryReportsByDomainAndPeriod(collection, domain, period.Value);

                var providerConnections = filteredReportInfos.SelectMany(
                    reportInfo => reportInfo.Report.Policies, 
                    (reportInfo, policySummary) => new
                    {
                        Provider = reportInfo.TlsReportSubmitter,
                        policySummary.Summary.TotalSuccessfulSessionCount,
                        policySummary.Summary.TotalFailureSessionCount,
                        policySummary.FailureDetails
                    }
                );

                var providerFailedSessionCounts = providerConnections.SelectMany(
                    x => x.FailureDetails, 
                    (parent, failureDetail) => new
                    {
                        parent.Provider,
                        failureDetail.ResultType,
                        failureDetail.FailedSessionCount
                    }
                );

                var failedSessionCountsByProviderAndType = providerFailedSessionCounts.GroupBy(x => new { x.Provider, x.ResultType });

                var failedSessionCountsSummed = failedSessionCountsByProviderAndType.Select(x => new
                {
                    x.Key.Provider,
                    x.Key.ResultType,
                    FailedSessionCount = x.Sum(y => y.FailedSessionCount)
                });

                var failedSessionsFromDatabase = failedSessionCountsSummed
                    .OrderByDescending(s => s.FailedSessionCount)
                    .Take(limit)
                    .ToList(); //Executes here

                var overallSessionsFromDatabase = providerConnections
                    .GroupBy(x => x.Provider)
                    .Select(x => new
                    {
                        Provider = x.Key,
                        TotalSessions = x.Sum(y => y.TotalSuccessfulSessionCount + y.TotalFailureSessionCount)
                    })
                    .ToList(); //Executes here

                _logger.LogInformation($"{nameof(GetFailures)} period {period} retrieved from database in {stopwatch.ElapsedMilliseconds}");

                // This join is executed in-memory, not in the database
                List<ProviderFailure> failedSessionPercentages = failedSessionsFromDatabase
                    .Join(
                        overallSessionsFromDatabase,
                        outer => outer.Provider,
                        inner => inner.Provider,
                        (outer, inner) => new ProviderFailure
                        {
                            Provider = outer.Provider,
                            ResultType = outer.ResultType,
                            Percent = outer.FailedSessionCount == 0 || inner.TotalSessions == 0 ? 0 : Math.Round((double)outer.FailedSessionCount / inner.TotalSessions * 100, 2)
                        }
                    )
                    .ToList();

                titleResults[period.Key] = failedSessionPercentages;
            }

            _logger.LogInformation($"{nameof(GetFailures)} completed in {stopwatch.ElapsedMilliseconds}");

            stopwatch.Stop();

            return titleResults;
        }

        public async Task<List<ProviderTotals>> GetTotals(string domain, Tuple<DateTime, DateTime> periodDateRange)
        {
            Stopwatch stopwatch = new Stopwatch();
            _logger.LogInformation($"Beginning {nameof(GetTotals)} for domain {domain}");
            stopwatch.Start();

            IMongoCollection<ReportInfo> collection = await GetCollection();

            IMongoQueryable<ReportInfo> filteredReportInfos = QueryReportsByDomainAndPeriod(collection, domain, periodDateRange);

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

        private async Task<IMongoCollection<ReportInfo>> GetCollection()
        {
            IMongoClient client = await _mongoClientProvider.GetMongoClient();

            IMongoDatabase database = client.GetDatabase(_config.Database);
            IMongoCollection<ReportInfo> collection = database.GetCollection<ReportInfo>(CollectionName);

            return collection;
        }

        private IMongoQueryable<ReportInfo> QueryReportsByDomainAndPeriod(IMongoCollection<ReportInfo> collection, string domain, Tuple<DateTime, DateTime> period)
        {
            var (reportWindowBeginInclusive, reportWindowEndExclusive) = period;

            IMongoQueryable<ReportInfo> filteredReportInfos = collection.AsQueryable()
                .Where(info => info.TlsReportDomain == domain &&
                           info.Report.DateRange.EndDatetime >= reportWindowBeginInclusive &&
                           info.Report.DateRange.EndDatetime < reportWindowEndExclusive);

            return filteredReportInfos;
        }
    }
}