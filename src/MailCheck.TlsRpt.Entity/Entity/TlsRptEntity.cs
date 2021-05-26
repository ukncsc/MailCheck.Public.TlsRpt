using System;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Common.Exception;
using MailCheck.TlsRpt.Contracts.Entity;
using MailCheck.TlsRpt.Contracts.Evaluator;
using MailCheck.TlsRpt.Contracts.Scheduler;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using MailCheck.TlsRpt.Entity.Config;
using MailCheck.TlsRpt.Entity.Dao;
using MailCheck.TlsRpt.Entity.Entity.Notifiers;
using Microsoft.Extensions.Logging;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.TlsRpt.Entity.Entity
{
    public class TlsRptEntity:
        IHandle<DomainCreated>,
        IHandle<TlsRptRecordExpired>,
        IHandle<TlsRptRecordsEvaluated>,
        IHandle<DomainDeleted>
    {
        private readonly ITlsRptEntityDao _dao;
        private readonly ILogger<TlsRptEntity> _log;
        private readonly ITlsRptEntityConfig _tlsRptEntityConfig;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IChangeNotifiersComposite _changeNotifiersComposite;

        public TlsRptEntity(ITlsRptEntityDao dao,
            ITlsRptEntityConfig tlsRptEntityConfig,
            IMessageDispatcher dispatcher, IChangeNotifiersComposite changeNotifiersComposite,
            ILogger<TlsRptEntity> log)
        {
            _dao = dao;
            _log = log;
            _tlsRptEntityConfig = tlsRptEntityConfig;
            _dispatcher = dispatcher;
            _changeNotifiersComposite = changeNotifiersComposite;
        }

        public async Task Handle(DomainCreated message)
        {
            string domain = message.Id.ToLower();

            TlsRptEntityState state = await _dao.Get(domain);

            if (state != null)
            {
                _log.LogInformation($"Ignoring {nameof(DomainCreated)} as TlsRptEntity already exists for {domain}.");
            }
            else
            {
                state = new TlsRptEntityState(domain, 1, TlsRptState.Created, DateTime.UtcNow);
                await _dao.Save(state);
                _log.LogInformation($"Created TlsRptEntity for {domain}.");
            }
            
            TlsRptEntityCreated tlsRptEntityCreated = new TlsRptEntityCreated(domain, state.Version);
            _dispatcher.Dispatch(tlsRptEntityCreated, _tlsRptEntityConfig.SnsTopicArn);
            _log.LogInformation($"Dispatched {nameof(TlsRptEntityCreated)}Created TlsRptEntity for {domain}.");
        }

        public async Task Handle(DomainDeleted message)
        {
            string domain = message.Id.ToLower();
            int rows = await _dao.Delete(domain);
            if (rows == 1)
            {
                _log.LogInformation($"Deleted TLS-RPT entity with id: {domain}.");
            }
            else
            {
                _log.LogInformation($"TLS-RPT entity already deleted with id: {domain}.");
            }
        }

        public async Task Handle(TlsRptRecordExpired message)
        {
            string domain = message.Id.ToLower();

            TlsRptEntityState state = await LoadState(domain, nameof(message));

            Message updatePollPending = state.UpdatePollPending();

            state.Version++;

            await _dao.Save(state);

            _dispatcher.Dispatch(updatePollPending, _tlsRptEntityConfig.SnsTopicArn);
        }

        public async Task Handle(TlsRptRecordsEvaluated message)
        {
            string domain = message.Id.ToLower();

            TlsRptEntityState state = await LoadState(domain, nameof(message));

            _changeNotifiersComposite.Handle(state, message);

            Message updateTlsRptEvaluation = state.UpdateTlsRptEvaluation(message.Records,
                message.Messages, message.Timestamp);

            state.Version++;

            await _dao.Save(state);

            _dispatcher.Dispatch(updateTlsRptEvaluation, _tlsRptEntityConfig.SnsTopicArn);

        }

        private async Task<TlsRptEntityState> LoadState(string id, string messageType)
        {
            TlsRptEntityState state = await _dao.Get(id);

            if (state == null)
            {
                _log.LogError("Ignoring {EventName} as TLS/RPT Entity does not exist for {Id}.", messageType, id);
                throw new MailCheckException(
                    $"Cannot handle event {messageType} as TLS/RPT Entity does not exists for {id}.");
            }

            return state;
        }
    }
}
