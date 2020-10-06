﻿using System;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Common.Exception;
using MailCheck.TlsRpt.Contracts.Entity;
using MailCheck.TlsRpt.Contracts.Evaluator;
using MailCheck.TlsRpt.Contracts.External;
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
            string messageId = message.Id.ToLower();

            TlsRptEntityState state = await _dao.Get(messageId);

            if (state != null)
            {
                _log.LogError("Ignoring {EventName} as TlsRptEntity already exists for {Id}.", nameof(DomainCreated), messageId);
                throw new MailCheckException($"Cannot handle event {nameof(DomainCreated)} as TlsRptEntity already exists for {messageId}.");
            }
            
            state = new TlsRptEntityState(messageId, 1, TlsRptState.Created, DateTime.UtcNow);
            await _dao.Save(state);
            TlsRptEntityCreated tlsRptEntityCreated = new TlsRptEntityCreated(messageId, state.Version);
            _dispatcher.Dispatch(tlsRptEntityCreated, _tlsRptEntityConfig.SnsTopicArn);
            _log.LogInformation("Created TlsRptEntity for {Id}.", messageId);
        }

        public async Task Handle(TlsRptRecordExpired message)
        {
            string messageId = message.Id.ToLower();

            TlsRptEntityState state = await LoadState(messageId, nameof(message));

            Message updatePollPending = state.UpdatePollPending();

            state.Version++;

            await _dao.Save(state);

            _dispatcher.Dispatch(updatePollPending, _tlsRptEntityConfig.SnsTopicArn);
        }

        public async Task Handle(TlsRptRecordsEvaluated message)
        {
            string messageId = message.Id.ToLower();

            TlsRptEntityState state = await LoadState(messageId, nameof(message));

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

        public async Task Handle(DomainDeleted message)
        {
            await _dao.Delete(message.Id);
            _log.LogInformation($"Deleted TLS/RPT entity with id: {message.Id}.");
        }
    }
}
