using System.Collections.Generic;
using System.Linq;
using MailCheck.TlsRpt.Poller.Domain;

namespace MailCheck.TlsRpt.Poller.Rules
{
    public class EvaluationResult<T>
    {
        public EvaluationResult(T item, params Error[] errors)
        {
            Item = item;
            Errors = errors.ToList();
        }

        public EvaluationResult(T item, List<Error> errors)
        {
            Item = item;
            Errors = errors;
        }

        public T Item { get; }

        public List<Error> Errors { get; }
    }
}