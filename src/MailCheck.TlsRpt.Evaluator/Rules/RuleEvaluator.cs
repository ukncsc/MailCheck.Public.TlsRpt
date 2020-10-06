using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Contracts.SharedDomain;

namespace MailCheck.TlsRpt.Evaluator.Rules
{
    public interface IRuleEvaluator<T>
    {
        Task<EvaluationResult<T>> Evaluate(T item);
    }

    public class RuleEvaluator<T> : IRuleEvaluator<T>
    {
        private readonly List<IRule<T>> _rules;

        public RuleEvaluator(IEnumerable<IRule<T>> rules)
        {
            _rules = rules.ToList();
        }

        public virtual async Task<EvaluationResult<T>> Evaluate(T item)
        {
            List<Message> errors = new List<Message>();
            foreach (IRule<T> rule in _rules)
            {
                List<Message> ruleErrors = await rule.Evaluate(item);

                if (ruleErrors.Any())
                {
                    errors.AddRange(ruleErrors);

                    if (rule.IsStopRule)
                    {
                        break;
                    }
                }
            }
            return new EvaluationResult<T>(item, errors);
        }
    }
}