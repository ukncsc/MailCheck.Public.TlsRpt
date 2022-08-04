using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Contracts.Findings;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Notifiers;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.TlsRpt.Reports.Contracts;
using MailCheck.TlsRpt.Reports.Entity.Config;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Reports.Entity.Entity.Notifiers
{
    public class FindingsChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IFindingsChangedNotifier _findingsChangedCalculator;
        private readonly ITlsRptReportsEntityConfig _tlsRptReportsEntityConfig;
        private readonly ILogger<FindingsChangedNotifier> _logger;

        public FindingsChangedNotifier(
            IMessageDispatcher dispatcher,
            IFindingsChangedNotifier findingsChangedCalculator,
            ITlsRptReportsEntityConfig tlsRptReportsEntityConfig,
            ILogger<FindingsChangedNotifier> logger)
        {
            _dispatcher = dispatcher;
            _findingsChangedCalculator = findingsChangedCalculator;
            _tlsRptReportsEntityConfig = tlsRptReportsEntityConfig;
            _logger = logger;
        }

        public void Handle(string domain, List<ReportsAdvisoryMessage> current, List<ReportsAdvisoryMessage> incoming)
        {
            List<Finding> currentFindings = MapToFindings(current, domain);
            List<Finding> incomingFindings = MapToFindings(incoming, domain);

            FindingsChanged findingsChanged = _findingsChangedCalculator.Process(domain, "TLS-RPT", currentFindings, incomingFindings);

            _logger.LogInformation($"Dispatching FindingsChanged for {domain}: {findingsChanged.Added?.Count} findings added, {findingsChanged.Sustained?.Count} findings sustained, {findingsChanged.Removed?.Count} findings removed");
            _dispatcher.Dispatch(findingsChanged, _tlsRptReportsEntityConfig.SnsTopicArn);
        }

        private List<Finding> MapToFindings(List<ReportsAdvisoryMessage> advisoryMessages, string domain)
        {
            List<ReportsAdvisoryMessage> messages = advisoryMessages ?? new List<ReportsAdvisoryMessage>();
            return messages.Select(msg => new Finding
            {
                Name = msg.Name,
                SourceUrl = $"https://{_tlsRptReportsEntityConfig.WebUrl}/app/domain-security/{domain}/tls-rpt",
                Title = msg.Text,
                EntityUri = $"domain:{domain}",
                Severity = AdvisoryMessageTypeToFindingSeverityMapping[msg.MessageType]
            }).ToList();
        }

        internal static readonly Dictionary<MessageType, string> AdvisoryMessageTypeToFindingSeverityMapping = new Dictionary<MessageType, string>
        {
            [MessageType.info] = "Informational",
            [MessageType.warning] = "Advisory",
            [MessageType.error] = "Urgent",
            [MessageType.success] = "Positive",
        };
    }
}