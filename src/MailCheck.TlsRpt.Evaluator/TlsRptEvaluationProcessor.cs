using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using MailCheck.TlsRpt.Evaluator.Explainers;
using MailCheck.TlsRpt.Evaluator.Rules;

namespace MailCheck.TlsRpt.Evaluator
{
    public interface ITlsRptEvaluationProcessor
    {
        Task<List<Message>> Process(TlsRptRecords tlsRptRecords);
    }

    public class TlsRptEvaluationProcessor : ITlsRptEvaluationProcessor
    {
        private readonly IEvaluator<TlsRptRecord> _evaluator;
        private readonly ITlsRptRecordExplainer _recordExplainer;

        public TlsRptEvaluationProcessor(IEvaluator<TlsRptRecord> evaluator,
            ITlsRptRecordExplainer recordExplainer)
        {
            _evaluator = evaluator;
            _recordExplainer = recordExplainer;
        }

        public async Task<List<Message>> Process(TlsRptRecords tlsRptRecords)
        {
            List<Message> errors = new List<Message>();

            if (tlsRptRecords != null)
            {
                foreach (TlsRptRecord tlsRptRecord in tlsRptRecords.Records)
                {
                    _recordExplainer.Process(tlsRptRecord);

                    EvaluationResult<TlsRptRecord> evaluationResult = await _evaluator.Evaluate(tlsRptRecord);
                    errors.AddRange(evaluationResult.Errors);
                }
            }

            return errors;
        }
    }
}