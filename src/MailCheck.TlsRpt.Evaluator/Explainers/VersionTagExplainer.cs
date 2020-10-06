using System.Linq;
using MailCheck.TlsRpt.Contracts.SharedDomain;

namespace MailCheck.TlsRpt.Evaluator.Explainers
{
    public class VersionTagExplainer : ITagExplainer
    {
        public void AddExplanation(TlsRptRecord record)
        {
            VersionTag version = record.Tags.OfType<VersionTag>().FirstOrDefault();

            if (version != null)
            {
                version.Explanation = TlsRptExplainerResource.VersionTlsRptExplanation;
            }
        }
    }
}
