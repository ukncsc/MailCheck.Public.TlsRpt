using System.Collections.Generic;
using System.Linq;
using MailCheck.TlsRpt.Contracts.SharedDomain;

namespace MailCheck.TlsRpt.Evaluator.Rules
{
    public class EvaluationResult<T>
    {
        public EvaluationResult(T item, params Message[] errors)
        {
            Item = item;
            Errors = errors.ToList();
        }

        public EvaluationResult(T item, List<Message> errors)
        {
            Item = item;
            Errors = errors;
        }

        public T Item { get; }

        public List<Message> Errors { get; }
    }
}