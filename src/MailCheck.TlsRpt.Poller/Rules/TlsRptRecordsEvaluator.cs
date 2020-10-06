using System.Threading.Tasks;
using MailCheck.TlsRpt.Poller.Domain;

namespace MailCheck.TlsRpt.Poller.Rules
{
    public interface ITlsRptRecordsEvaluator : IEvaluator<TlsRptRecords>
    {

    }

    public class TlsRptRecordsEvaluator : ITlsRptRecordsEvaluator
    {
        private readonly IEvaluator<TlsRptRecords> _recordsEvaluator;
        private readonly IEvaluator<TlsRptRecord> _recordEvaluator;

        public TlsRptRecordsEvaluator(IEvaluator<TlsRptRecords> recordsEvaluator,
            IEvaluator<TlsRptRecord> recordEvaluator)
        {
            _recordsEvaluator = recordsEvaluator;
            _recordEvaluator = recordEvaluator;
        }

        public async Task<EvaluationResult<TlsRptRecords>> Evaluate(TlsRptRecords item)
        {
            EvaluationResult<TlsRptRecords> recordsEvaluationResult = await _recordsEvaluator.Evaluate(item);

            foreach (TlsRptRecord tlsRptRecord in item.Records)
            {
                EvaluationResult<TlsRptRecord> tlsRptRecordEvaluationResult = await _recordEvaluator.Evaluate(tlsRptRecord);
                recordsEvaluationResult.Errors.AddRange(tlsRptRecordEvaluationResult.Errors);
            }

            return recordsEvaluationResult;
        }
    }
}