using System.Collections.Generic;
using MailCheck.TlsRpt.Poller.Domain;
using MailCheck.TlsRpt.Poller.Rules;

namespace MailCheck.TlsRpt.Poller.Parsing
{
    public interface ITlsRptRecordsParser
    {
        EvaluationResult<TlsRptRecords> Parse(TlsRptRecordInfos tlsRptRecordInfos);
    }

    public class TlsRptRecordsParser : ITlsRptRecordsParser
    {
        private readonly ITlsRptRecordParser _parser;

        public TlsRptRecordsParser(ITlsRptRecordParser parser)
        {
            _parser = parser;
        }
        public EvaluationResult<TlsRptRecords> Parse(TlsRptRecordInfos tlsRptRecordInfos)
        {
            List<TlsRptRecord> records = new List<TlsRptRecord>();
            List<Error> errors = new List<Error>();
            foreach (TlsRptRecordInfo tlsRptRecordInfo in tlsRptRecordInfos.RecordsInfos)
            {
                EvaluationResult<TlsRptRecord> tlsRptRecord = _parser.Parse(tlsRptRecordInfo);
                records.Add(tlsRptRecord.Item);
                errors.AddRange(tlsRptRecord.Errors);
            }

            TlsRptRecords tlsRptRecords = new TlsRptRecords(tlsRptRecordInfos.Domain, records, tlsRptRecordInfos.MessageSize);
            return new EvaluationResult<TlsRptRecords>(tlsRptRecords, errors);
        }
    }
}