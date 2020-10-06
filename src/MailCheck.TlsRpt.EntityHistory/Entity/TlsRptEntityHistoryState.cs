using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.TlsRpt.EntityHistory.Entity
{
    public class TlsRptHistoryRecord
    {
        public TlsRptHistoryRecord(DateTime startDate, DateTime? endDate, List<string> tlsRptRecords = null)
        {
            StartDate = startDate;
            EndDate = endDate;
            TlsRptRecords = tlsRptRecords ?? new List<string>();
        }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> TlsRptRecords { get; set; }
    }

    public class TlsRptEntityHistoryState 
    {
        public TlsRptEntityHistoryState(
            string id,
            List<TlsRptHistoryRecord> records = null)
        {
            Id = id;
            TlsRptHistory = records ?? new List<TlsRptHistoryRecord>();
        }

        public string Id { get; set; }
        public List<TlsRptHistoryRecord> TlsRptHistory { get; set; }

        public bool UpdateHistory(List<string> polledRecords, DateTime timeStamp)
        {
            bool hasChanged;

            TlsRptHistoryRecord currentRecord = TlsRptHistory.FirstOrDefault();

            List<string> cleanedPolledRecords = polledRecords.Select(x => x.Replace("; ", ";")).ToList();
            if (currentRecord == null)
            {
                TlsRptHistory.Add(new TlsRptHistoryRecord(timeStamp, null, cleanedPolledRecords));
                hasChanged = true;
            }
            else
            {
                List<string> currentRecords = currentRecord.TlsRptRecords.Select(x => x).ToList();
                
                hasChanged = !(cleanedPolledRecords.Count == currentRecords.Count && currentRecords.All(cleanedPolledRecords.Contains));

                if (hasChanged)
                {
                    currentRecord.EndDate = timeStamp;

                    TlsRptHistory.Insert(0, new TlsRptHistoryRecord(timeStamp, null, cleanedPolledRecords));
                }
            }

            return hasChanged;
        }
    }
}
