using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.TestSupport;
using MailCheck.Common.Util;
using MailCheck.TlsRpt.Migration;
using MailCheck.TlsRpt.Scheduler.Config;
using MailCheck.TlsRpt.Scheduler.Dao;
using MailCheck.TlsRpt.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.TlsRpt.Scheduler.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class TlsRptPeriodicSchedulerDaoTests : DatabaseTestBase
    {
        private TlsRptPeriodicSchedulerDao _dao;
        private IDatabase _database;
        private IClock _clock;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            TruncateDatabase().Wait();
            _database = A.Fake<IDatabase>();
            _clock = A.Fake<IClock>();

            ITlsRptPeriodicSchedulerConfig config = A.Fake<ITlsRptPeriodicSchedulerConfig>();
            A.CallTo(() => config.RefreshIntervalSeconds).Returns(0);
            A.CallTo(() => config.DomainBatchSize).Returns(10);

            _dao = new TlsRptPeriodicSchedulerDao(config, _database, _clock);
        }

        [Test]
        public async Task ItShouldReturnNothingIfThereAreNoExpiredRecords()
        {
            List<TlsRptSchedulerState> states = await _dao.GetExpiredTlsRptRecords();
            Assert.That(states.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task ItShouldReturnAllExpiredRecords()
        {
            await Insert("ncsc.gov.uk", DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)));

            List<TlsRptSchedulerState> state = await _dao.GetExpiredTlsRptRecords();

            Assert.That(state[0].Id, Is.EqualTo("ncsc.gov.uk"));
        }

        [Test]
        public async Task ItShouldUpdateTheLastCheckedTime()
        {
            await Insert("ncsc.gov.uk", DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)));

            DateTime current = await GetLastChecked("ncsc.gov.uk");

            await _dao.UpdateLastChecked(new List<TlsRptSchedulerState> { new TlsRptSchedulerState("ncsc.gov.uk") });

            DateTime updated = await GetLastChecked("ncsc.gov.uk");

            Assert.That(updated, Is.GreaterThan(current));
        }

        protected override string GetDatabaseName() => "tlsrpt";

        protected override Assembly GetSchemaAssembly() => Assembly.GetAssembly(typeof(Migrator));

        private Task Insert(string domain, DateTime lastChecked) =>
            MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO tls_rpt_scheduled_records (id, last_checked) VALUES (@domain, @last_checked)",
                new MySqlParameter("domain", domain),
                new MySqlParameter("last_checked", lastChecked));

        private async Task<DateTime> GetLastChecked(string domain) =>
            (DateTime)await MySqlHelper.ExecuteScalarAsync(ConnectionString,
                "SELECT last_checked FROM tls_rpt_scheduled_records WHERE id = @domain",
                new MySqlParameter("domain", domain));

        private Task TruncateDatabase() =>
            MySqlHelper.ExecuteNonQueryAsync(ConnectionString, "DELETE FROM tls_rpt_scheduled_records;");

    }
}
