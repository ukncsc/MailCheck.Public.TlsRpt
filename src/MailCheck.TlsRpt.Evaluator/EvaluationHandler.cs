using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Contracts.Evaluator;
using MailCheck.TlsRpt.Contracts.Poller;
using MailCheck.TlsRpt.Evaluator.Config;
using Message = MailCheck.TlsRpt.Contracts.SharedDomain.Message;

namespace MailCheck.TlsRpt.Evaluator
{
    public class EvaluationHandler : IHandle<TlsRptRecordsPolled>
    {
        private readonly ITlsRptEvaluationProcessor _tlsRptEvaluationProcessor;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ITlsRptEvaluatorConfig _config;

        public EvaluationHandler(ITlsRptEvaluationProcessor tlsRptEvaluationProcessor,
            IMessageDispatcher dispatcher,
            ITlsRptEvaluatorConfig config)
        {
            _tlsRptEvaluationProcessor = tlsRptEvaluationProcessor;
            _dispatcher = dispatcher;
            _config = config;
        }

        public async Task Handle(TlsRptRecordsPolled message)
        {
            List<Message> messages = message.Messages ?? new List<Message>();

            messages.AddRange(await _tlsRptEvaluationProcessor.Process(message.TlsRptRecords));

            _dispatcher.Dispatch(
                new TlsRptRecordsEvaluated(message.Id, message.TlsRptRecords, messages, message.Timestamp), _config.SnsTopicArn);
        }
    }
}
