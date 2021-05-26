using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Data.Util;
using MailCheck.TlsRpt.Scheduler.Config;
using MailCheck.TlsRpt.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;
using Dapper;
using MailCheck.Common.Data;
using MailCheck.Common.Util;
using System;

namespace MailCheck.TlsRpt.Scheduler.Dao
{
    public interface ITlsRptPeriodicSchedulerDao
    {
        Task UpdateLastChecked(List<TlsRptSchedulerState> entitiesToUpdate);
        Task<List<TlsRptSchedulerState>> GetExpiredTlsRptRecords();
    }
    
    public class TlsRptPeriodicSchedulerDao : ITlsRptPeriodicSchedulerDao
    {
        private readonly ITlsRptPeriodicSchedulerConfig _config;
        private readonly IDatabase _database;
        private readonly IClock _clock;

        public TlsRptPeriodicSchedulerDao(
            ITlsRptPeriodicSchedulerConfig config,
            IDatabase database,
            IClock clock)
        {
            _config = config;
            _database = database;
            _clock = clock;
        }

        public async Task<List<TlsRptSchedulerState>> GetExpiredTlsRptRecords()
        {
            DateTime nowMinusInterval = _clock.GetDateTimeUtc().AddSeconds(- _config.RefreshIntervalSeconds);

            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                var records = (await connection.QueryAsync<string>(
                    TlsRptPeriodicSchedulerDaoResources.SelectTlsRptRecordsToSchedule,
                    new {now_minus_interval = nowMinusInterval, limit = _config.DomainBatchSize})).ToList();

                return records.Select(record => new TlsRptSchedulerState(record)).ToList();
            }
        }

        public async Task UpdateLastChecked(List<TlsRptSchedulerState> entitiesToUpdate)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                var parameters = entitiesToUpdate.Select(ent => new { id = ent.Id, lastChecked = GetAdjustedLastCheckedTime() }).ToArray();
                await connection.ExecuteAsync(TlsRptPeriodicSchedulerDaoResources.UpdateTlsRptRecordsLastCheckedDistributed, parameters);
            }
        }

        private DateTime GetAdjustedLastCheckedTime()
        {
            return _clock.GetDateTimeUtc().AddSeconds(-(new Random().NextDouble() * 3600));
        }
    }
}
