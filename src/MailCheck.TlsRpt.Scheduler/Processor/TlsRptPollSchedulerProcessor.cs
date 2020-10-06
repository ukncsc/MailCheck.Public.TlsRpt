using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Scheduler.Config;
using MailCheck.TlsRpt.Scheduler.Dao;
using MailCheck.TlsRpt.Scheduler.Dao.Model;
using MailCheck.TlsRpt.Scheduler.Mapping;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Scheduler.Processor
{
    public class TlsRptPollSchedulerProcessor : IProcess
    {
        private readonly ITlsRptPeriodicSchedulerDao _dao;
        private readonly IMessagePublisher _publisher;
        private readonly ITlsRptPeriodicSchedulerConfig _config;
        private readonly ILogger<TlsRptPollSchedulerProcessor> _log;

        public TlsRptPollSchedulerProcessor(ITlsRptPeriodicSchedulerDao dao,
            IMessagePublisher publisher,
            ITlsRptPeriodicSchedulerConfig config,
            ILogger<TlsRptPollSchedulerProcessor> log)
        {
            _dao = dao;
            _publisher = publisher;
            _config = config;
            _log = log;
        }

        public async Task<ProcessResult> Process()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<TlsRptSchedulerState> expiredRecords = await _dao.GetExpiredTlsRptRecords();

            _log.LogInformation($"Found {expiredRecords.Count} expired records.");

            if (expiredRecords.Any())
            {
                expiredRecords
                    .Select(_ => _publisher.Publish(_.ToTlsRptRecordExpiredMessage(), _config.PublisherConnectionString))
                    .Batch(10)
                    .ToList()
                    .ForEach(async _ => await Task.WhenAll(_));

                await _dao.UpdateLastChecked(expiredRecords);

                _log.LogInformation($"Processing for domains {string.Join(',', expiredRecords.Select(_ => _.Id))} took {stopwatch.Elapsed}.");
            }

            stopwatch.Stop();

            return expiredRecords.Any()
                ? ProcessResult.Continue
                : ProcessResult.Stop;
        }
    }
}
