using System;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.Entity;
using MailCheck.TlsRpt.Contracts.Poller;
using MailCheck.TlsRpt.Poller.Config;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Mapping;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Poller
{
    public class PollHandler : IHandle<TlsRptPollPending>
    {
        private readonly ITlsRptProcessor _processor;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ITlsRptPollerConfig _config;
        private readonly ILogger<PollHandler> _log;

        public PollHandler(ITlsRptProcessor processor, 
            IMessageDispatcher dispatcher, 
            ITlsRptPollerConfig config,
            ILogger<PollHandler> log)
        {
            _processor = processor;
            _dispatcher = dispatcher;
            _config = config;
            _log = log;
        }

        public async Task Handle(TlsRptPollPending message)
        {
            try
            {
                TlsRptPollResult tlsRptPollResult = await _processor.Process(message.Id);

                TlsRptRecordsPolled tlsRptRecordsPolled = tlsRptPollResult.ToTlsRptRecordsPolled();

                _dispatcher.Dispatch(tlsRptRecordsPolled, _config.SnsTopicArn);
            }
            catch(Exception e)
            {
                string error = $"Error occurred polling domain {message.Id}";
                _log.LogError(e, error);
                throw;
            }
        }
    }
}
