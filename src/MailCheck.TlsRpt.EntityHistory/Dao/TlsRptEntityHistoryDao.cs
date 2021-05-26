using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.TlsRpt.EntityHistory.Entity;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.TlsRpt.EntityHistory.Dao
{
    public interface ITlsRptEntityHistoryDao
    {
        Task<TlsRptEntityHistoryState> Get(string domain);
        Task Save(TlsRptEntityHistoryState state);
    }

    public class TlsRptEntityHistoryDao : ITlsRptEntityHistoryDao
    {
        private readonly IConnectionInfoAsync _connectionInfoAsync;

        public TlsRptEntityHistoryDao(IConnectionInfoAsync connectionInfoAsync)
        {
            _connectionInfoAsync = connectionInfoAsync;
        }

        public async Task<TlsRptEntityHistoryState> Get(string domain)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string state = (string)await MySqlHelper.ExecuteScalarAsync(connectionString, TlsRptEntityHistoryDaoResouces.SelectTlsRptHistoryEntity,
                new MySqlParameter("domain", domain));

            return state == null
                ? null
                : JsonConvert.DeserializeObject<TlsRptEntityHistoryState>(state);
        }

        public async Task Save(TlsRptEntityHistoryState state)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string serializedState = JsonConvert.SerializeObject(state, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                TlsRptEntityHistoryDaoResouces.InsertTlsRptEntityHistory,
                new MySqlParameter("domain", state.Id.ToLower()),
                new MySqlParameter("state", serializedState));
        }
    }
}
