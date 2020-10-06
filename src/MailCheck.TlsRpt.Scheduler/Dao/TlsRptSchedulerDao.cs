using System;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.TlsRpt.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.TlsRpt.Scheduler.Dao
{
    public interface ITlsRptSchedulerDao
    {
        Task<TlsRptSchedulerState> Get(string domain);
        Task Save(TlsRptSchedulerState state);
        Task Delete(string domain);
    }

    public class TlsRptSchedulerDao : ITlsRptSchedulerDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;

        public TlsRptSchedulerDao(IConnectionInfoAsync connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<TlsRptSchedulerState> Get(string domain)
        {
            string id = (string)await MySqlHelper.ExecuteScalarAsync(
                await _connectionInfo.GetConnectionStringAsync(),
                TlsRptSchedulerDaoResources.SelectTlsRptRecord,
                new MySqlParameter("id", domain));

            return id == null
                ? null
                : new TlsRptSchedulerState(id);
        }

        public async Task Save(TlsRptSchedulerState state)
        {
            int numberOfRowsAffected = await MySqlHelper.ExecuteNonQueryAsync(
                await _connectionInfo.GetConnectionStringAsync(),
                TlsRptSchedulerDaoResources.InsertTlsRptRecord,
                new MySqlParameter("id", state.Id.ToLower()));

            if (numberOfRowsAffected == 0)
            {
                throw new InvalidOperationException($"Didn't save duplicate {nameof(TlsRptSchedulerState)} for {state.Id}");
            }
        }

        public async Task Delete(string domain)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            await MySqlHelper.ExecuteNonQueryAsync(connectionString, TlsRptSchedulerDaoResources.DeleteTlsRptRecord, new MySqlParameter("id", domain));
        }
    }
}
