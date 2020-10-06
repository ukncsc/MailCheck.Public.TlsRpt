using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Contracts.SharedDomain;

namespace MailCheck.TlsRpt.Evaluator.Rules
{
    public interface IRule<in T>
    {
        Task<List<Message>> Evaluate(T t);
        int SequenceNo { get; }
        bool IsStopRule { get; }
    }
}