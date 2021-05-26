using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.Poller;
using MailCheck.TlsRpt.EntityHistory.Config;
using MailCheck.TlsRpt.EntityHistory.Dao;
using MailCheck.TlsRpt.EntityHistory.Service;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.EntityHistory.Entity
{
    public class TlsRptEntityHistory : IHandle<DomainCreated>,
        IHandle<TlsRptRecordsPolled>
    {
        private readonly ITlsRptEntityHistoryDao _dao;
        private readonly ITlsRptEntityHistoryConfig _tlsRptEntityHistoryConfig;
        private readonly ILogger<TlsRptEntityHistory> _log;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ITlsRptRuaService _ruaService;

        public TlsRptEntityHistory(
            IMessageDispatcher dispatcher,
            ILogger<TlsRptEntityHistory> log,
            ITlsRptEntityHistoryDao dao,
            ITlsRptEntityHistoryConfig tlsRptEntityHistoryConfig,
            ITlsRptRuaService ruaService)
        {
            _dispatcher = dispatcher;
            _dao = dao;
            _tlsRptEntityHistoryConfig = tlsRptEntityHistoryConfig;
            _log = log;
            _ruaService = ruaService;
        }

        public async Task Handle(DomainCreated message)
        {
            string domain = message.Id.ToLower();

            TlsRptEntityHistoryState state = await _dao.Get(domain);

            if (state == null)
            {
                state = new TlsRptEntityHistoryState(domain);
                await _dao.Save(state);
                _log.LogInformation("Created TlsRptHistoryEntity for {Id}.", domain);
            }
            else
            {
                _log.LogInformation("Ignoring {EventName} as TlsRptHistoryEntity already exists for {Id}.",
                    nameof(DomainCreated), domain);
            }
        }

        public async Task Handle(TlsRptRecordsPolled message)
        {
            string messageId = message.Id.ToLower();

            TlsRptEntityHistoryState tlsRptEntityHistory = await LoadHistoryState(messageId);

            List<string> records = new List<string>();

            message.TlsRptRecords?.Records.ForEach(x => records.Add(x.Record));
            
            if (tlsRptEntityHistory.UpdateHistory(records, message.Timestamp))
            {
                _log.LogInformation("TLS-RPT record has changed for {Id}", messageId);

                await _dao.Save(tlsRptEntityHistory);

                records.ForEach(x => _ruaService.Process(messageId, x));
            }
            else
            {
                _log.LogInformation("No TLS-RPT Record change for {Id}", messageId);
            }
        
    }

        private async Task<TlsRptEntityHistoryState> LoadHistoryState(string id)
        {
            TlsRptEntityHistoryState tlsRptEntityHistoryState =
                await _dao.Get(id) ?? new TlsRptEntityHistoryState(id);

            return tlsRptEntityHistoryState;
        }
    }
}