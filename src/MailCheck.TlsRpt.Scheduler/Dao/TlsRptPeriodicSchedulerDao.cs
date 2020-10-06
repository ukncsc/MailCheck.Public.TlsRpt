using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MailCheck.TlsRpt.Scheduler.Config;
using MailCheck.TlsRpt.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.TlsRpt.Scheduler.Dao
{
    public interface ITlsRptPeriodicSchedulerDao
    {
        Task UpdateLastChecked(List<TlsRptSchedulerState> entitiesToUpdate);
        Task<List<TlsRptSchedulerState>> GetExpiredTlsRptRecords();
    }

    public class TlsRptPeriodicSchedulerDao : ITlsRptPeriodicSchedulerDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;
        private readonly ITlsRptPeriodicSchedulerConfig _config;

        public TlsRptPeriodicSchedulerDao(IConnectionInfoAsync connectionInfo, ITlsRptPeriodicSchedulerConfig config)
        {
            _connectionInfo = connectionInfo;
            _config = config;
        }

        public async Task<List<TlsRptSchedulerState>> GetExpiredTlsRptRecords()
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            List<TlsRptSchedulerState> results = new List<TlsRptSchedulerState>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(connectionString,
                TlsRptPeriodicSchedulerDaoResources.SelectTlsRptRecordsToSchedule,
                new MySqlParameter("refreshIntervalSeconds", _config.RefreshIntervalSeconds),
                new MySqlParameter("limit", _config.DomainBatchSize)))
            {
                while (await reader.ReadAsync())
                {
                    results.Add(CreateTlsRptSchedulerState(reader));
                }
            }

            return results;
        }

        public async Task UpdateLastChecked(List<TlsRptSchedulerState> entitiesToUpdate)
        {
            string query = string.Format(TlsRptPeriodicSchedulerDaoResources.UpdateTlsRptRecordsLastChecked,
                string.Join(',', entitiesToUpdate.Select((_, i) => $"@domainName{i}")));

            MySqlParameter[] parameters = entitiesToUpdate
                .Select((_, i) => new MySqlParameter($"domainName{i}", _.Id.ToLower()))
                .ToArray();

            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            await MySqlHelper.ExecuteNonQueryAsync(connectionString, query, parameters);
        }

        private TlsRptSchedulerState CreateTlsRptSchedulerState(DbDataReader reader)
        {
            return new TlsRptSchedulerState(reader.GetString("id"));
        }
    }
}
