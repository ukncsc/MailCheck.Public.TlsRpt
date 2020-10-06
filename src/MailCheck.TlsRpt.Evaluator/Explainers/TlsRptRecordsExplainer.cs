using System.Collections.Generic;
using MailCheck.TlsRpt.Contracts.SharedDomain;

namespace MailCheck.TlsRpt.Evaluator.Explainers
{
    public interface ITlsRptRecordsExplainer
    {
        void Process(IList<TlsRptRecord> records);
    }

    public class TlsRptRecordsExplainer : ITlsRptRecordsExplainer
    {
        private readonly ITlsRptRecordExplainer _tlsRptRecordExplainer;
        
        public TlsRptRecordsExplainer(ITlsRptRecordExplainer tlsRptRecordExplainer)
        {
            _tlsRptRecordExplainer = tlsRptRecordExplainer;
        }

        public void Process(IList<TlsRptRecord> records)
        {
            foreach (var record in records)
            {
                _tlsRptRecordExplainer.Process(record);
            }
        }
    }
}
