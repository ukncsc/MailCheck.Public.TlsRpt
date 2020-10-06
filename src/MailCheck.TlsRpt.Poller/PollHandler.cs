using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.Entity;
using MailCheck.TlsRpt.Contracts.Poller;
using MailCheck.TlsRpt.Poller.Config;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Mapping;

namespace MailCheck.TlsRpt.Poller
{
    public class PollHandler : IHandle<TlsRptPollPending>
    {
        private readonly ITlsRptProcessor _processor;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ITlsRptPollerConfig _config;

        public PollHandler(ITlsRptProcessor processor, 
            IMessageDispatcher dispatcher, 
            ITlsRptPollerConfig config)
        {
            _processor = processor;
            _dispatcher = dispatcher;
            _config = config;
        }

        public async Task Handle(TlsRptPollPending message)
        {
            TlsRptPollResult tlsRptPollResult = await _processor.Process(message.Id);

            TlsRptRecordsPolled tlsRptRecordsPolled = tlsRptPollResult.ToTlsRptRecordsPolled();

            _dispatcher.Dispatch(tlsRptRecordsPolled, _config.SnsTopicArn);
        }
    }
}
