using System;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Poller.Config;
using MailCheck.TlsRpt.Poller.Dns;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Exceptions;
using MailCheck.TlsRpt.Poller.Parsing;
using MailCheck.TlsRpt.Poller.Rules;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Poller
{
    public interface ITlsRptProcessor
    {
        Task<TlsRptPollResult> Process(string domain);
    }

    public class TlsRptProcessor : ITlsRptProcessor
    {
        private readonly IDnsClient _dnsClient;
        private readonly ITlsRptRecordsParser _parser;
        private readonly ITlsRptRecordsEvaluator _evaluator;
        private readonly ILogger<TlsRptProcessor> _log;

        public TlsRptProcessor(IDnsClient dnsClient,
            ITlsRptRecordsParser parser,
            ITlsRptRecordsEvaluator evaluator, 
            ITlsRptPollerConfig config, ILogger<TlsRptProcessor> log)
        {
            _dnsClient = dnsClient;
            _parser = parser;
            _evaluator = evaluator;
            _log = log;
        }

        public async Task<TlsRptPollResult> Process(string domain)
        {
            TlsRptRecordInfos tlsRptRecordInfos = await _dnsClient.GetTlsRptRecords(domain);

            if (tlsRptRecordInfos.HasError)
            {
                string message = $"Failed TLS/RPT record query for {domain} with error {tlsRptRecordInfos.Error}";

                _log.LogError($"{message} {Environment.NewLine} Audit Trail: {tlsRptRecordInfos.AuditTrail}");

                return new TlsRptPollResult(domain, tlsRptRecordInfos.Error);
            }

            if (tlsRptRecordInfos.RecordsInfos.Count == 0 ||
                tlsRptRecordInfos.RecordsInfos.TrueForAll(x => string.IsNullOrWhiteSpace(x.Record)))
            {
                _log.LogInformation(
                    $"TLS/RPT records missing or empty for {domain}, Name server: {tlsRptRecordInfos.NameServer}");
            }
            
            EvaluationResult<TlsRptRecords> parsingResult = _parser.Parse(tlsRptRecordInfos);

            EvaluationResult<TlsRptRecords> evaluationResult = await _evaluator.Evaluate(parsingResult.Item);

            return new TlsRptPollResult(evaluationResult.Item, parsingResult.Errors.Concat(evaluationResult.Errors).ToList());
        }
    }
}