using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Domain.Errors.Rules;

namespace MailCheck.TlsRpt.Poller.Rules.Records
{
    public class OnlyOneTlsRptRecord : IRule<TlsRptRecords>
    {
        public Task<List<Error>> Evaluate(TlsRptRecords t)
        {
            List<Error> errors = new List<Error>();
            if (t.Records.Count > 1)
            {
                errors.Add(new OnlyOneTlsRptRecordError());
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 2;
        public bool IsStopRule => false;
    }
}