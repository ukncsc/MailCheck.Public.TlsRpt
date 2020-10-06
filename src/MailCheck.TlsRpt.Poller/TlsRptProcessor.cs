using System.Linq;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Poller.Config;
using MailCheck.TlsRpt.Poller.Dns;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Exceptions;
using MailCheck.TlsRpt.Poller.Parsing;
using MailCheck.TlsRpt.Poller.Rules;

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
        private readonly ITlsRptPollerConfig _config;

        public TlsRptProcessor(IDnsClient dnsClient,
            ITlsRptRecordsParser parser,
            ITlsRptRecordsEvaluator evaluator, 
            ITlsRptPollerConfig config)
        {
            _dnsClient = dnsClient;
            _parser = parser;
            _evaluator = evaluator;
            _config = config;
        }

        public async Task<TlsRptPollResult> Process(string domain)
        {
            TlsRptRecordInfos tlsRptRecordInfos = await _dnsClient.GetTlsRptRecords(domain);

            if (!_config.AllowNullResults && (tlsRptRecordInfos.HasError ||
                                              tlsRptRecordInfos.RecordsInfos.TrueForAll(x => string.IsNullOrWhiteSpace(x.Record))))
            {
                throw new TlsRptPollerException($"Unable to retrieve TLS-RPT records for {domain}.");
            }

            if (tlsRptRecordInfos.HasError)
            {
                return new TlsRptPollResult(domain, tlsRptRecordInfos.Error);
            }

            EvaluationResult<TlsRptRecords> parsingResult = _parser.Parse(tlsRptRecordInfos);

            EvaluationResult<TlsRptRecords> evaluationResult = await _evaluator.Evaluate(parsingResult.Item);

            return new TlsRptPollResult(evaluationResult.Item, parsingResult.Errors.Concat(evaluationResult.Errors).ToList());
        }
    }
}