using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MailCheck.Common.TestSupport;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using MailCheck.TlsRpt.Entity.Dao;
using MailCheck.TlsRpt.Entity.Entity;
using MailCheck.TlsRpt.Migration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NUnit.Framework;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.TlsRpt.Entity.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class TlsRptEntityDaoTests : DatabaseTestBase
    {
        private const string Id = "abc.com";

        private TlsRptEntityDao _dao;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            IConnectionInfoAsync connectionInfoAsync = A.Fake<IConnectionInfoAsync>();
            A.CallTo(() => connectionInfoAsync.GetConnectionStringAsync()).Returns(ConnectionString);

            _dao = new TlsRptEntityDao(connectionInfoAsync);
        }

        [Test]
        public async Task GetNoStateExistsReturnsNull()
        {
            TlsRptEntityState state = await _dao.Get(Id);
            Assert.That(state, Is.Null);
        }

        [Test]
        public async Task GetStateExistsReturnsState()
        {
            TlsRptEntityState state = new TlsRptEntityState(Id, 1, TlsRptState.PollPending, DateTime.UtcNow);

            await Insert(state);

            TlsRptEntityState stateFromDatabase = await _dao.Get(Id);

            Assert.That(stateFromDatabase.Id, Is.EqualTo(state.Id));
            Assert.That(stateFromDatabase.Version, Is.EqualTo(state.Version));
        }

        [Test]
        public async Task SaveStateExistsStateIsUpdated()
        {
            TlsRptEntityState state = new TlsRptEntityState(Id, 1, TlsRptState.PollPending, DateTime.UtcNow);

            await _dao.Save(state);

            state = new TlsRptEntityState(Id, 2, TlsRptState.PollPending, DateTime.UtcNow);

            await _dao.Save(state);

            List<TlsRptEntityState> states = await SelectAll(Id);

            Assert.That(states.Count, Is.EqualTo(1));
            Assert.That(states[0].Id, Is.EqualTo(state.Id));
            Assert.That(states[0].Version, Is.EqualTo(state.Version));
        }

        [Test]
        public async Task SaveDuplicateEntryThrows()
        {
            TlsRptEntityState state1 = new TlsRptEntityState(Id, 1, TlsRptState.PollPending, DateTime.UtcNow);

            await _dao.Save(state1);
            Assert.ThrowsAsync<InvalidOperationException>(() => _dao.Save(state1));
        }
      
        protected override string GetDatabaseName() => "tlsrptentity";

        protected override Assembly GetSchemaAssembly()
        {
            return Assembly.GetAssembly(typeof(Migrator));
        }

        #region TestSupport

        private async Task Insert(TlsRptEntityState state)
        {
            await MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO `tls_rpt_entity`(`id`,`version`,`state`)VALUES(@domain,@version,@state)",
                new MySqlParameter("domain", state.Id),
                new MySqlParameter("version", state.Version),
                new MySqlParameter("state", JsonConvert.SerializeObject(state)));
        }

        private async Task<List<TlsRptEntityState>> SelectAll(string id)
        {
            List<TlsRptEntityState> list = new List<TlsRptEntityState>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(ConnectionString,
                @"SELECT state FROM tls_rpt_entity WHERE id = @domain ORDER BY version;",
                new MySqlParameter("domain", id)))
            {
                while (reader.Read())
                {
                    string state = reader.GetString("state");

                    if (!string.IsNullOrWhiteSpace(state))
                    {
                        list.Add(JsonConvert.DeserializeObject<TlsRptEntityState>(state));
                    }
                }
            }

            return list;
        }

        #endregion
    }
}
