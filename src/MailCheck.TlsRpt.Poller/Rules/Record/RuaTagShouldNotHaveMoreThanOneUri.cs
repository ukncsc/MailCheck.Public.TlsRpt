using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Domain.Errors.Rules;

namespace MailCheck.TlsRpt.Poller.Rules.Record
{
    public class RuaTagShouldNotHaveMoreThanOneUri : IRule<TlsRptRecord>
    {
        public Task<List<Error>> Evaluate(TlsRptRecord t)
        {
            List<Error> errors = new List<Error>();

            RuaTag ruaTag = t.Tags.OfType<RuaTag>().FirstOrDefault();

            if (ruaTag != null && ruaTag.Uris.Count > 1)
            {
                errors.Add(new RuaTagShouldNotHaveMoreThanOneUriError(ruaTag.Uris.Count));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 3;
        public bool IsStopRule => false;
    }
}