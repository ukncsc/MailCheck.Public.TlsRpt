using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Reports.Contracts;
using MailCheck.TlsRpt.Reports.Entity.Config;
using MailCheck.TlsRpt.Reports.Entity.Entity.Notifications;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Reports.Entity.Entity.Notifiers
{
    public class AdvisoryChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly ITlsRptReportsEntityConfig _tlsRptReportsEntityConfig;
        private readonly IEqualityComparer<AdvisoryMessage> _messageEqualityComparer;
        private readonly ILogger<AdvisoryChangedNotifier> _log;

        public AdvisoryChangedNotifier(IMessageDispatcher dispatcher,
            ITlsRptReportsEntityConfig tlsRptReportsEntityConfig,
            IEqualityComparer<AdvisoryMessage> messageEqualityComparer,
            ILogger<AdvisoryChangedNotifier> log)
        {
            _dispatcher = dispatcher;
            _tlsRptReportsEntityConfig = tlsRptReportsEntityConfig;
            _messageEqualityComparer = messageEqualityComparer;
            _log = log;
        }

        public void Handle(string domain, List<ReportsAdvisoryMessage> currentAdvisories, List<ReportsAdvisoryMessage> newAdvisories)
        {
            currentAdvisories = currentAdvisories ?? new List<ReportsAdvisoryMessage>();
            newAdvisories = newAdvisories ?? new List<ReportsAdvisoryMessage>();

            _log.LogInformation($"Dispatching advisories for {domain}");

            List<AdvisoryMessage> addedMessages =
                newAdvisories.Except(currentAdvisories, _messageEqualityComparer).ToList();
            if (addedMessages.Any())
            {
                TlsRptAdvisoryAdded advisoryAdded = new TlsRptAdvisoryAdded(
                    domain,
                    addedMessages);
                _dispatcher.Dispatch(advisoryAdded, _tlsRptReportsEntityConfig.SnsTopicArn);
                _log.LogInformation($"Dispatched {advisoryAdded.Messages.Count} added advisories for {domain}");
            }

            List<AdvisoryMessage> removedMessages =
                currentAdvisories.Except(newAdvisories, _messageEqualityComparer).ToList();
            if (removedMessages.Any())
            {
                TlsRptAdvisoryRemoved advisoryRemoved = new TlsRptAdvisoryRemoved(
                    domain,
                    removedMessages);
                _dispatcher.Dispatch(advisoryRemoved, _tlsRptReportsEntityConfig.SnsTopicArn);
                _log.LogInformation($"Dispatched {advisoryRemoved.Messages.Count} removed advisories for {domain}");
            }

            List<AdvisoryMessage> sustainedMessages =
                currentAdvisories.Intersect(newAdvisories, _messageEqualityComparer).ToList();
            if (sustainedMessages.Any())
            {
                TlsRptAdvisorySustained advisorySustained = new TlsRptAdvisorySustained(
                    domain,
                    sustainedMessages);
                _dispatcher.Dispatch(advisorySustained, _tlsRptReportsEntityConfig.SnsTopicArn);
                _log.LogInformation($"Dispatched {advisorySustained.Messages.Count} sustained advisories for {domain}");
            }

            _log.LogInformation($"Advisories successfully dispatched for {domain}");
        }
    }
}