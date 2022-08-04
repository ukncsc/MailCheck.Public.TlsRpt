using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Contracts.External;
using MailCheck.TlsRpt.Reports.Contracts;
using MailCheck.TlsRpt.Reports.Entity.Config;
using MailCheck.TlsRpt.Reports.Entity.Dao;
using MailCheck.TlsRpt.Reports.Entity.Entity.Notifiers;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Reports.Entity.Entity
{
    public class TlsRptReportsEntity :
        IHandle<TlsRptReportsScheduledReminder>,
        IHandle<DomainCreated>,
        IHandle<DomainDeleted>
    {
        private static readonly int[] Periods = new int[] { 2, 14, 90 };

        private readonly ITlsRptReportsEntityDao _dao;
        private readonly IReportsDataDao _reportsDao;
        private readonly ILogger<TlsRptReportsEntity> _log;
        private readonly IEvaluator<DomainProvidersResults> _evaluator;
        private readonly IChangeNotifiersComposite _changeNotifiersComposite;
        private readonly IClock _clock;
        private readonly ITlsRptReportsEntityConfig _config;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IReportPeriodCalculator _reportPeriodCalculator;

        public TlsRptReportsEntity(ITlsRptReportsEntityDao dao,
            IReportsDataDao reportsDao,
            ILogger<TlsRptReportsEntity> log,
            IChangeNotifiersComposite changeNotifiersComposite,
            IEvaluator<DomainProvidersResults> evaluator,
            IClock clock,
            ITlsRptReportsEntityConfig config,
            IMessageDispatcher dispatcher,
            IReportPeriodCalculator reportPeriodCalculator)
        {
            _dao = dao;
            _reportsDao = reportsDao;
            _log = log;
            _changeNotifiersComposite = changeNotifiersComposite;
            _evaluator = evaluator;
            _clock = clock;
            _config = config;
            _dispatcher = dispatcher;
            _reportPeriodCalculator = reportPeriodCalculator;
        }

        public async Task Handle(DomainCreated message)
        {
            string domain = message.Id.ToLower();

            await LoadState(domain);
        }

        public async Task Handle(TlsRptReportsScheduledReminder message)
        {
            string domain = message.ResourceId.ToLower();

            _log.LogInformation($"Handling TlsRptScheduledReminder for {domain}");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            TlsRptReportsEntityState state = await LoadState(domain);

            IDictionary<int, Tuple<DateTime, DateTime>> periodDateRanges = _reportPeriodCalculator.GetDateRangesForPeriods(Periods);

            List<ProviderTotals> providerTotals = await _reportsDao.GetTotals(domain, periodDateRanges[90]);
            Dictionary<int, List<ProviderFailure>> providerFailures = await _reportsDao.GetFailures(domain, 30, periodDateRanges);

            DomainProvidersResults results = new DomainProvidersResults
            {
                Periods = periodDateRanges,
                Domain = domain,
                ProviderTotals = providerTotals,
                ProviderFailures = providerFailures
            };

            EvaluationResult<DomainProvidersResults> evaluationResult = await _evaluator.Evaluate(results);
            List<ReportsAdvisoryMessage> incomingAdvisories = evaluationResult.AdvisoryMessages.Select(msg => msg as ReportsAdvisoryMessage).ToList();

            _changeNotifiersComposite.Handle(domain, state.AdvisoryMessages, incomingAdvisories);

            state.AdvisoryMessages = incomingAdvisories;

            await _dao.Update(state);

            _log.LogInformation($"TLS-RPT Reports advisories generated for {domain} in {stopwatch.ElapsedMilliseconds}");
            stopwatch.Stop();
        }

        public async Task Handle(DomainDeleted message)
        {
            string domain = message.Id.ToLower();
            _log.LogInformation($"Handling DomainDeleted for {domain}");

            await _dao.Delete(domain);

            _dispatcher.Dispatch(new DeleteScheduledReminder(Guid.NewGuid().ToString(), "TlsRptReports", domain), _config.SnsTopicArn);
            _log.LogInformation($"Dispatched DeleteScheduledReminder for {domain}");
        }

        private async Task<TlsRptReportsEntityState> LoadState(string domain)
        {
            TlsRptReportsEntityState state = await _dao.Read(domain);

            if (state == null)
            {
                DateTime currentTime = _clock.GetDateTimeUtc();

                state = new TlsRptReportsEntityState
                {
                    Domain = domain,
                    Version = 1,
                    AdvisoryMessages = new List<ReportsAdvisoryMessage>(),
                    Created = currentTime,
                    LastUpdated = currentTime
                };

                await _dao.Create(state);

                _dispatcher.Dispatch(new CreateScheduledReminder(
                Guid.NewGuid().ToString(),
                "TlsRptReports",
                domain,
                default), _config.SnsTopicArn);
                _log.LogInformation($"Dispatched CreateScheduleReminder for {domain}");

                return state;
            }

            return state;
        }
    }
}
