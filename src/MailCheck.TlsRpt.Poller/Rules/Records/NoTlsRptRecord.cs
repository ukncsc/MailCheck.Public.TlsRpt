using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Domain.Errors.Rules;

namespace MailCheck.TlsRpt.Poller.Rules.Records
{
    public class NoTlsRptRecord : IRule<TlsRptRecords>
    {
        public Task<List<Error>> Evaluate(TlsRptRecords t)
        {
            List<Error> errors = new List<Error>();
            if (!t.Records.Any())
            {
                errors.Add(new NoTlsRptError(t.Domain));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 1;
        public bool IsStopRule => false;
    }
}
