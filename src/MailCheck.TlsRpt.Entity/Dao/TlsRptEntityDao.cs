using System;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.TlsRpt.Entity.Entity;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.TlsRpt.Entity.Dao
{
    public interface ITlsRptEntityDao
    {
        Task<TlsRptEntityState> Get(string domain);
        Task Save(TlsRptEntityState state);
        Task<int> Delete(string domain);
    }

    public class TlsRptEntityDao : ITlsRptEntityDao
    {
        private readonly IConnectionInfoAsync _connectionInfoAsync;

        public TlsRptEntityDao(IConnectionInfoAsync connectionInfoAsync)
        {
            _connectionInfoAsync = connectionInfoAsync;
        }

        public async Task<TlsRptEntityState> Get(string domain)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string state = (string)await MySqlHelper.ExecuteScalarAsync(connectionString, TlsRptEntityDaoResouces.SelectTlsRptEntity,
                new MySqlParameter("domain", domain));

            return state == null
                ? null
                : JsonConvert.DeserializeObject<TlsRptEntityState>(state);
        }

        public async Task Save(TlsRptEntityState state)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string serializedState = JsonConvert.SerializeObject(state);

            int rowsAffected = await MySqlHelper.ExecuteNonQueryAsync(connectionString, TlsRptEntityDaoResouces.InsertTlsRptEntity,
                new MySqlParameter("domain", state.Id.ToLower()),
                new MySqlParameter("version", state.Version),
                new MySqlParameter("state", serializedState));

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    $"Didn't update TlsRptEntityState because version {state.Version} has already been persisted.");
            }
        }

        public async Task<int> Delete(string domain)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            return await MySqlHelper.ExecuteNonQueryAsync(connectionString, TlsRptEntityDaoResouces.DeleteTlsRptEntity, new MySqlParameter("id", domain));
        }
    }
}
