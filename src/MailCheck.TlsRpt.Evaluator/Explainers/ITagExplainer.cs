using MailCheck.TlsRpt.Contracts.SharedDomain;

namespace MailCheck.TlsRpt.Evaluator.Explainers
{
    public interface ITagExplainer
    {
        void AddExplanation(TlsRptRecord record);
    }
}
