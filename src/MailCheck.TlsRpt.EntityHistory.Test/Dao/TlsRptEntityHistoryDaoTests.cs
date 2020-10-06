using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MailCheck.Common.TestSupport;
using MailCheck.TlsRpt.EntityHistory.Dao;
using MailCheck.TlsRpt.EntityHistory.Entity;
using MailCheck.TlsRpt.Migration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NUnit.Framework;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.TlsRpt.EntityHistory.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class TlsRptEntityHistoryDaoTests : DatabaseTestBase
    {
        private const string Id = "abc.com";

        private ITlsRptEntityHistoryDao _dao;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            IConnectionInfoAsync connectionInfoAsync = A.Fake<IConnectionInfoAsync>();
            A.CallTo(() => connectionInfoAsync.GetConnectionStringAsync()).Returns(ConnectionString);

            _dao = new TlsRptEntityHistoryDao(connectionInfoAsync);
        }

        [Test]
        public async Task GetNoStateExistsReturnsNull()
        {
            TlsRptEntityHistoryState state = await _dao.Get(Id);
            Assert.That(state, Is.Null);
        }

        [Test]
        public async Task GetStateExistsReturnsState()
        {
            string record1 = "v=TLSRPTv1;rua=mailto:tlsrpt@example.com";

            TlsRptEntityHistoryState state = new TlsRptEntityHistoryState(Id,
                new List<TlsRptHistoryRecord> { new TlsRptHistoryRecord(DateTime.UtcNow.AddDays(-1), null, new List<string> { record1 }) });
            
            await Insert(state);

            TlsRptEntityHistoryState stateFromDatabase = await _dao.Get(Id);

            Assert.That(stateFromDatabase.Id, Is.EqualTo(state.Id));
            Assert.That(stateFromDatabase.TlsRptHistory.Count, Is.EqualTo(state.TlsRptHistory.Count));
            Assert.That(stateFromDatabase.TlsRptHistory[0].TlsRptRecords.Count, Is.EqualTo(state.TlsRptHistory[0].TlsRptRecords.Count));
            Assert.That(stateFromDatabase.TlsRptHistory[0].TlsRptRecords[0], Is.EqualTo(state.TlsRptHistory[0].TlsRptRecords[0]));
        }

        [Test]
        public async Task HistoryIsSavedForChanges()
        {
            string record1 = "v=TLSRPTv1;rua=mailto:tlsrpt@example.com";
            string record2 = "v=TLSRPTv1;rua=mailto:tlsrpt2@example.com";

            TlsRptEntityHistoryState state = new TlsRptEntityHistoryState(Id,
                new List<TlsRptHistoryRecord> { new TlsRptHistoryRecord(DateTime.UtcNow.AddDays(-1), null, new List<string> { record1 }) });

            await _dao.Save(state);

            TlsRptEntityHistoryState state2 = (await SelectAllHistory(Id)).First();
            state2.TlsRptHistory[0].EndDate = DateTime.UtcNow;
            state2.TlsRptHistory.Insert(0, new TlsRptHistoryRecord(DateTime.UtcNow, null, new List<string> { record2 }));

            await _dao.Save(state2);

            List<TlsRptEntityHistoryState> historyStates = await SelectAllHistory(Id);
            Assert.That(historyStates[0].TlsRptHistory.Count, Is.EqualTo(2));

            Assert.That(historyStates[0].TlsRptHistory[0].EndDate, Is.Null);
            Assert.That(historyStates[0].TlsRptHistory[0].TlsRptRecords.Count, Is.EqualTo(1));
            Assert.That(historyStates[0].TlsRptHistory[0].TlsRptRecords[0], Is.EqualTo(record2));

            Assert.That(historyStates[0].TlsRptHistory[1].EndDate, Is.Not.Null);
            Assert.That(historyStates[0].TlsRptHistory[1].TlsRptRecords.Count, Is.EqualTo(1));
            Assert.That(historyStates[0].TlsRptHistory[1].TlsRptRecords[0], Is.EqualTo(record1));
        }

        protected override string GetDatabaseName() => "tls_rpt_entity";

        protected override Assembly GetSchemaAssembly()
        {
            return Assembly.GetAssembly(typeof(Migrator));
        }

        #region TestSupport

        private async Task Insert(TlsRptEntityHistoryState state)
        {
            await MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO `tls_rpt_entity_history`(`id`,`state`)VALUES(@domain,@state)",
                new MySqlParameter("domain", state.Id),
                new MySqlParameter("state", JsonConvert.SerializeObject(state)));
        }

        private async Task<List<TlsRptEntityHistoryState>> SelectAllHistory(string id)
        {
            List<TlsRptEntityHistoryState> list = new List<TlsRptEntityHistoryState>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(ConnectionString,
                @"SELECT state FROM tls_rpt_entity_history WHERE id = @domain ORDER BY id;",
                new MySqlParameter("domain", id)))
            {
                while (reader.Read())
                {
                    string state = reader.GetString("state");

                    if (!string.IsNullOrWhiteSpace(state))
                    {
                        list.Add(JsonConvert.DeserializeObject<TlsRptEntityHistoryState>(state));
                    }
                }
            }

            return list;
        }

        #endregion
    }
}
