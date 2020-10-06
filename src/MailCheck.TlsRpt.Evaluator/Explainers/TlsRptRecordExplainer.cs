using System.Collections.Generic;
using MailCheck.TlsRpt.Contracts.SharedDomain;

namespace MailCheck.TlsRpt.Evaluator.Explainers
{
    public interface ITlsRptRecordExplainer
    {
        void Process(TlsRptRecord record);
    }

    public class TlsRptRecordExplainer : ITlsRptRecordExplainer
    {
        private readonly IEnumerable<ITagExplainer> _tagExplainers;

        public TlsRptRecordExplainer(IEnumerable<ITagExplainer> tagExplainers)
        {
            _tagExplainers = tagExplainers;
        }

        public void Process(TlsRptRecord record)
        {
            foreach (var tagExplainer in _tagExplainers)
            {
                tagExplainer.AddExplanation(record);
            }
        }
    }
}
