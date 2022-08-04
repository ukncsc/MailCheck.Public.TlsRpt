using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Contracts.Findings;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Notifiers;
using MailCheck.TlsRpt.Contracts.Evaluator;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using MailCheck.TlsRpt.Entity.Config;
using Microsoft.Extensions.Logging;
using ErrorMessage = MailCheck.TlsRpt.Contracts.SharedDomain.Message;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.TlsRpt.Entity.Entity.Notifiers
{
    public class FindingsChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IFindingsChangedNotifier _findingsChangedCalculator;
        private readonly ITlsRptEntityConfig _tlsRptEntityConfig;
        private readonly ILogger<FindingsChangedNotifier> _logger;

        public FindingsChangedNotifier(
            IMessageDispatcher dispatcher,
            IFindingsChangedNotifier findingsChangedCalculator,
            ITlsRptEntityConfig tlsRptEntityConfig,
            ILogger<FindingsChangedNotifier> logger)
        {
            _dispatcher = dispatcher;
            _findingsChangedCalculator = findingsChangedCalculator;
            _tlsRptEntityConfig = tlsRptEntityConfig;
            _logger = logger;
        }

        public void Handle(TlsRptEntityState state, Message message)
        {
            string messageId = state.Id.ToLower();

            if (message is TlsRptRecordsEvaluated evaluationResult)
            {
                FindingsChanged findingsChanged = _findingsChangedCalculator.Process(messageId, "TLS-RPT",
                    ExtractFindingsFromState(messageId, state),
                    ExtractFindingsFromResult(messageId, evaluationResult));

                _logger.LogInformation($"Dispatching FindingsChanged for {messageId}: {findingsChanged.Added?.Count} findings added, {findingsChanged.Sustained?.Count} findings sustained, {findingsChanged.Removed?.Count} findings removed");
                _dispatcher.Dispatch(findingsChanged, _tlsRptEntityConfig.SnsTopicArn);
            }
        }

        private IList<Finding> ExtractFindingsFromState(string domain, TlsRptEntityState state)
        {
            var rootMessages = state.Messages?.Where(_ => _.MessageDisplay != MessageDisplay.Prompt).ToList() ?? new List<ErrorMessage>();

            return ExtractFindingsFromMessages(domain, rootMessages);
        }

        private IList<Finding> ExtractFindingsFromResult(string domain, TlsRptRecordsEvaluated result)
        {
            var rootMessages = result.Messages?.Where(_ => _.MessageDisplay != MessageDisplay.Prompt).ToList() ?? new List<ErrorMessage>();

            return ExtractFindingsFromMessages(domain, rootMessages);
        }

        private List<Finding> ExtractFindingsFromMessages(string domain, List<ErrorMessage> rootMessages)
        {
            List<Finding> findings = rootMessages.Select(msg => new Finding
            {
                Name = msg.Name,
                SourceUrl = $"https://{_tlsRptEntityConfig.WebUrl}/app/domain-security/{domain}/tls-rpt",
                Title = msg.Text,
                EntityUri = $"domain:{domain}",
                Severity = AdvisoryMessageTypeToFindingSeverityMapping[msg.MessageType]
            }).ToList();

            return findings;
        }

        internal static readonly Dictionary<MessageType, string> AdvisoryMessageTypeToFindingSeverityMapping = new Dictionary<MessageType, string>
        {
            [MessageType.info] = "Informational",
            [MessageType.warning] = "Advisory",
            [MessageType.error] = "Urgent",
            [MessageType.positive] = "Positive",
        };
    }
}
