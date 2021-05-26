using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.Entity;
using MailCheck.TlsRpt.Scheduler.Config;
using MailCheck.TlsRpt.Scheduler.Dao;
using MailCheck.TlsRpt.Scheduler.Dao.Model;
using MailCheck.TlsRpt.Scheduler.Mapping;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Scheduler.Handler
{
    public class TlsRptSchedulerHandler : IHandle<TlsRptEntityCreated>, IHandle<DomainDeleted>
    {
        private readonly ITlsRptSchedulerDao _dao;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ITlsRptSchedulerConfig _config;
        private readonly ILogger<TlsRptSchedulerHandler> _log;

        public TlsRptSchedulerHandler(ITlsRptSchedulerDao dao,
            IMessageDispatcher dispatcher,
            ITlsRptSchedulerConfig config,
            ILogger<TlsRptSchedulerHandler> log)
        {
            _dao = dao;
            _dispatcher = dispatcher;
            _config = config;
            _log = log;
        }

        public async Task Handle(TlsRptEntityCreated message)
        {
            string domain = message.Id.ToLower();
            TlsRptSchedulerState state = await _dao.Get(domain);

            if (state == null)
            {
                state = new TlsRptSchedulerState(domain);

                await _dao.Save(state);

                _dispatcher.Dispatch(state.ToTlsRptRecordExpiredMessage(), _config.PublisherConnectionString);

                _log.LogInformation($"New {nameof(TlsRptSchedulerState)} saved for {domain}");
            }
            else
            {
                _log.LogInformation($"{nameof(TlsRptSchedulerState)} already exists for {domain}");
            }
        }

        public async Task Handle(DomainDeleted message)
        {
            string domain = message.Id.ToLower();
            int rows = await _dao.Delete(domain);
            if (rows == 1)
            {
                _log.LogInformation($"Deleted schedule for TLSRPT entity with id: {domain}.");
            }
            else
            {
                _log.LogInformation($"Schedule already deleted for TLSRPT entity with id: {domain}.");
            }
        }
    }
}
