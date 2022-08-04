using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.TlsRpt.Reports.Contracts;
using MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809;
using MailCheck.TlsRpt.Reports.Entity.Dao;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Reports.Entity.Entity.EvaluationRules
{
    public class TlsFailures : IRule<DomainProvidersResults>
    {
        private readonly ILogger<TlsFailures> _logger;

        private Guid TwoDaysSuccess = new Guid("81daf9c9-ee69-4220-a9af-4262fcd089cc");
        private Guid TwoDaysInfo = new Guid("9c2fd5e9-cef9-465b-945f-7aaed7416d62");
        private Guid TwoDaysWarning = new Guid("907af89b-dd38-4d73-9085-c9de21b30182");
        private Guid TwoDaysError = new Guid("39ad1004-e37e-45d5-9f27-b388b5fbeaf5");
        private Guid FourteenDaysSuccess = new Guid("f25e812d-99b7-4272-bd72-129a6889fd27");
        private Guid FourteenDaysInfo = new Guid("ecf33ec5-28bb-419a-8dd6-13c86ee89315");
        private Guid FourteenDaysWarning = new Guid("a29d3cc2-f64e-493d-8680-fb88ce2da89d");
        private Guid FourteenDaysError = new Guid("ac7fb401-c6fc-4e11-a6d8-3b65a18b11a7");
        private Guid NinetyDaysSuccess = new Guid("358b0f8f-22fe-441b-84a8-555167c991ab");
        private Guid NinetyDaysInfo = new Guid("d3c985c4-92e2-457d-94a1-88c4259d8665");
        private Guid NinetyDaysWarning = new Guid("4db39e11-b80d-4c6e-b822-e4f919610eb4");
        private Guid NinetyDaysError = new Guid("1225ebd3-e308-4417-b490-8a723885ca78");

        public TlsFailures(ILogger<TlsFailures> logger) 
        {
            _logger = logger;
        }

        public int SequenceNo => 1;

        public bool IsStopRule => false;

        public Task<List<AdvisoryMessage>> Evaluate(DomainProvidersResults results)
        {
            string domain = results.Domain;

            _logger.LogInformation($"Generating TLS Failures advisories for {domain}");

            List<ProviderTotals> providerTotals = results.ProviderTotals;

            _logger.LogInformation($"{providerTotals.Count} provider totals found for {domain}");

            List<AdvisoryMessage> advisoryMessages = new List<AdvisoryMessage>();

            IEnumerable<TitleResult> titleResults = results.Periods.Select(period =>
            {
                var (periodStart, _) = period.Value;
                List<ProviderTotals> periodTotals = providerTotals
                    .Where(y => y?.ReportEndDate >= periodStart).ToList();

                if (periodTotals.Count == 0)
                {
                    return null;
                }

                int totalFailures = periodTotals.Sum(y => y.TotalFailureSessionCount);
                int totalSessions = periodTotals.Sum(y => y.TotalSuccessfulSessionCount) + totalFailures;

                double percentageFailures;
                if (totalSessions == 0 || totalFailures == 0)
                {
                    percentageFailures = 0;
                }
                else
                {
                    percentageFailures = Math.Round((double)totalFailures / totalSessions * 100, 2);
                }

                return new TitleResult
                {
                    Period = period.Key,
                    DateRange = period.Value,
                    ProviderCount = periodTotals.Where(x => x.TotalFailureSessionCount > 0).GroupBy(y => y.Provider).Count(),
                    PercentageFailures = percentageFailures
                };
            })
            .Where(result => result != null)
            .RationaliseZeros()
            .Reverse();

            foreach (TitleResult result in titleResults)
            {
                int period = result.Period;
                var (periodStart, periodEndExclusive) = result.DateRange;
                var periodEnd = periodEndExclusive.AddDays(-1);

                MessageType messageType = GetMessageType(result.PercentageFailures);
                var guidAndName = GetGuidAndName(period, messageType);
                Guid guid = guidAndName.Guid;
                string name = guidAndName.Name;

                if (result.PercentageFailures > 0)
                {
                    Dictionary<string, List<ProviderFailure>> providerFailures = results.ProviderFailures[period]
                        .OrderBy(x => x.Provider)
                        .GroupBy(x => x.Provider)
                        .ToDictionary(x => x.Key, x => x.OrderByDescending(f => f.Percent).ToList());

                    string advisoryText = $"In the last {period} days, {result.ProviderCount} email providers reported TLS failures accounting for {result.PercentageFailures}% of all inbound sessions";
                    StringBuilder advisoryMarkdown = new StringBuilder();

                    foreach (KeyValuePair<string, List<ProviderFailure>> provider in providerFailures)
                    {
                        advisoryMarkdown.Append($"- {provider.Key} reports that:\n");

                        advisoryMarkdown.AppendJoin('\n', provider.Value.Select(providerFailure =>
                            $"    - {providerFailure.Percent}% of their TLS sessions failed because {GetDescription(providerFailure.ResultType)}"
                        ));

                        advisoryMarkdown.Append("\n");
                    }

                    advisoryMarkdown.Append($"\nReports for the period {periodStart:d MMM yyyy} to {periodEnd:d MMM yyyy}");

                    AdvisoryMessage advisoryMessage = new ReportsAdvisoryMessage(guid, name, messageType, advisoryText, advisoryMarkdown.ToString(), $"{period}-days");
                    advisoryMessages.Add(advisoryMessage);
                }
                else
                {
                    string advisoryText = $"In the last {period} days, no TLS failures were reported.";

                    StringBuilder advisoryMarkdown = new StringBuilder();
                    advisoryMarkdown.Append($"- No failures were reported\n");
                    advisoryMarkdown.Append("\n");
                    advisoryMarkdown.Append($"\nReports for the period {periodStart:d MMM yyyy} to {periodEnd:d MMM yyyy}");

                    AdvisoryMessage advisoryMessage = new ReportsAdvisoryMessage(guid, name, messageType, advisoryText, advisoryMarkdown.ToString(), $"{period}-days");
                    advisoryMessages.Add(advisoryMessage);
                }
            }

            return Task.FromResult(advisoryMessages);
        }

        private (Guid Guid, string Name) GetGuidAndName(int period, MessageType messageType)
        {
            if (period == 2)
            {
                switch (messageType)
                {
                    case MessageType.success:
                        return (TwoDaysSuccess, "mailcheck.tlsrpt.zeroTlsFailuresIn2Days");
                    case MessageType.info:
                        return (TwoDaysInfo, "mailcheck.tlsrpt.lessThan10PercentTlsFailuresIn2Days");
                    case MessageType.warning:
                        return (TwoDaysWarning, "mailcheck.tlsrpt.lessThan50PercentTlsFailuresIn2Days");
                    case MessageType.error:
                        return (TwoDaysError, "mailcheck.tlsrpt.moreThan50PercentTlsFailuresIn2Days");
                }
            }

            if (period == 14)
            {
                switch (messageType)
                {
                    case MessageType.success:
                        return (FourteenDaysSuccess, "mailcheck.tlsrpt.zeroTlsFailuresIn14Days");
                    case MessageType.info:
                        return (FourteenDaysInfo, "mailcheck.tlsrpt.lessThan10PercentTlsFailuresIn14Days");
                    case MessageType.warning:
                        return (FourteenDaysWarning, "mailcheck.tlsrpt.lessThan50PercentTlsFailuresIn14Days");
                    case MessageType.error:
                        return (FourteenDaysError, "mailcheck.tlsrpt.moreThan50PercentTlsFailuresIn14Days");
                }
            }

            if (period == 90)
            {
                switch (messageType)
                {
                    case MessageType.success:
                        return (NinetyDaysSuccess, "mailcheck.tlsrpt.zeroTlsFailuresIn90Days");
                    case MessageType.info:
                        return (NinetyDaysInfo, "mailcheck.tlsrpt.lessThan10PercentTlsFailuresIn90Days");
                    case MessageType.warning:
                        return (NinetyDaysWarning, "mailcheck.tlsrpt.lessThan50PercentTlsFailuresIn90Days");
                    case MessageType.error:
                        return (NinetyDaysError, "mailcheck.tlsrpt.moreThan50PercentTlsFailuresIn90Days");
                }
            }

            throw new Exception("Invalid period or message type");
        }

        private MessageType GetMessageType(double percentageFailures)
        {
            if (percentageFailures == 0)
            {
                return MessageType.success;
            }

            if (percentageFailures < 10)
            {
                return MessageType.info;
            }

            if (percentageFailures < 50)
            {
                return MessageType.warning;
            }

            return MessageType.error;
        }

        private string GetDescription(ResultType resultType)
        {
            switch (resultType)
            {
                case ResultType.StartTlsNotSupported:
                    return "the recipient MX did not support STARTTLS";
                case ResultType.CertificateHostMismatch:
                    return "the certificate presented did not adhere to the constraints specified in the MTA-STS or DANE policy";
                case ResultType.CertificateExpired:
                    return "the certificate has expired";
                case ResultType.CertificateNotTrusted:
                    return "the certificate was not trusted";
                case ResultType.ValidationFailure:
                    return "a negotiation failure occurred";
                case ResultType.TlsaInvalid:
                case ResultType.DnssecInvalid:
                case ResultType.DaneRequired:
                    return "a DANE failure occurred";
                case ResultType.StsPolicyFetchError:
                case ResultType.StsPolicyInvalid:
                case ResultType.StsWebpkiInvalid:
                    return "an MTA-STS failure occurred";
                default:
                    return "of an unclassified error";
            }
        }
    }

    static class TitleResultExtensions
    {
        public static IEnumerable<TitleResult> RationaliseZeros(this IEnumerable<TitleResult> titleResults)
        {
            IEnumerable<TitleResult> sortedResults = titleResults.OrderByDescending(o => o.Period);
            bool haveEncounteredZero = false;
            List<TitleResult> agg = new List<TitleResult>();
            foreach (var result in sortedResults)
            {
                if (haveEncounteredZero)
                {
                    return agg;
                }

                if (result.PercentageFailures == 0)
                {
                    haveEncounteredZero = true;
                }

                agg.Add(result);
            }

            return agg;
        }
    }
}
