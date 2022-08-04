using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Util;

namespace MailCheck.TlsRpt.Reports.Entity.Dao
{
    public interface IReportPeriodCalculator
    {
        IDictionary<int, Tuple<DateTime, DateTime>> GetDateRangesForPeriods(int[] periodInDays);
    }

    public class ReportPeriodCalculator : IReportPeriodCalculator
    {
        private readonly IClock _clock;
        private const int ReportDeliveryBufferInDays = 1; //0 would indicate yesterday midnight

        public ReportPeriodCalculator(IClock clock)
        {
            _clock = clock;
        }

        public IDictionary<int, Tuple<DateTime, DateTime>> GetDateRangesForPeriods(int[] periodsInDays)
        {
            var today = _clock.GetDateTimeUtc().Date;

            return periodsInDays.ToDictionary(period => period, period =>
            {
                DateTime reportWindowEndExclusive = today.AddDays(-ReportDeliveryBufferInDays);
                DateTime reportWindowBeginInclusive = reportWindowEndExclusive.AddDays(-period);
                return Tuple.Create(reportWindowBeginInclusive, reportWindowEndExclusive);
            });
        }

    }
}