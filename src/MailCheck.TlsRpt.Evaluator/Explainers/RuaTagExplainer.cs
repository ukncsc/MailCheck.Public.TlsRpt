using System;
using System.Linq;
using MailCheck.TlsRpt.Contracts.SharedDomain;

namespace MailCheck.TlsRpt.Evaluator.Explainers
{
    public class RuaTagExplainer : ITagExplainer
    {
        public void AddExplanation(TlsRptRecord record)
        {
            RuaTag ruaTag = record.Tags.OfType<RuaTag>().FirstOrDefault();

            if (ruaTag != null)
            {
                if (!ruaTag.Uris.Any(_ => _.Type == nameof(UnknownUri) || _.Type == nameof(MalformedUri)))
                {
                    string uris = string.Join(Environment.NewLine, ruaTag.Uris.Select(_ => $"{_.Value}"));
                    ruaTag.Explanation = string.Format(TlsRptExplainerResource.RuaExplanation, uris);
                }
            }
        }
    }
}
