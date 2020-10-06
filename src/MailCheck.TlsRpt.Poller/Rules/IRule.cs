using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Poller.Domain;

namespace MailCheck.TlsRpt.Poller.Rules
{
    public interface IRule<in T>
    {
        Task<List<Error>> Evaluate(T t);
        int SequenceNo { get; }
        bool IsStopRule { get; }
    }
}