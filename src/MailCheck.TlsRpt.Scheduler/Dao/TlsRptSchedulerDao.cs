using System;
using System.Threading.Tasks;
using Dapper;
using MailCheck.Common.Data;
using MailCheck.TlsRpt.Scheduler.Dao.Model;

namespace MailCheck.TlsRpt.Scheduler.Dao
{
    public interface ITlsRptSchedulerDao
    {
        Task<TlsRptSchedulerState> Get(string domain);
        Task Save(TlsRptSchedulerState state);
        Task<int> Delete(string domain);
    }

    public class TlsRptSchedulerDao : ITlsRptSchedulerDao
    {
        private readonly IDatabase _database;

        public TlsRptSchedulerDao(IDatabase database)
        {
            _database = database;
        }

        public async Task<TlsRptSchedulerState> Get(string domain)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                string id = await connection.QueryFirstOrDefaultAsync<string>(
                    TlsRptSchedulerDaoResources.SelectTlsRptRecord,
                    new {id = domain});
                
                return id == null
                    ? null
                    : new TlsRptSchedulerState(id);
            }
        }

        public async Task Save(TlsRptSchedulerState state)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                int numberOfRowsAffected = await connection.ExecuteAsync(TlsRptSchedulerDaoResources.InsertTlsRptRecord,
                    new { id = state.Id.ToLower() });

                if (numberOfRowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        $"Didn't save duplicate {nameof(TlsRptSchedulerState)} for {state.Id}");
                }
            }
        }

        public async Task<int> Delete(string domain)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                return await connection.ExecuteAsync(TlsRptSchedulerDaoResources.DeleteTlsRptRecord,
                    new {id = domain});
            }
        }
    }
}
