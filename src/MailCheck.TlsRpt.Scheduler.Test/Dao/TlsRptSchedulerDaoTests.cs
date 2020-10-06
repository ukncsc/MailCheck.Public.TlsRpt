using System;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.TestSupport;
using MailCheck.TlsRpt.Migration;
using MailCheck.TlsRpt.Scheduler.Dao;
using MailCheck.TlsRpt.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.TlsRpt.Scheduler.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class TlsRptSchedulerDaoTests : DatabaseTestBase
    {
        private TlsRptSchedulerDao _dao;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            TruncateDatabase().Wait();

            IConnectionInfoAsync connectionInfo = A.Fake<IConnectionInfoAsync>();
            A.CallTo(() => connectionInfo.GetConnectionStringAsync()).Returns(ConnectionString);

            _dao = new TlsRptSchedulerDao(connectionInfo);
        }

        [Test]
        public async Task ItShouldReturnNullIfTheStateDoesntExist()
        {
            TlsRptSchedulerState state = await _dao.Get("ncsc.gov.uk");
            Assert.That(state, Is.Null);
        }

        [Test]
        public async Task ItShouldGetTheStateIfItExists()
        {
            await Insert("ncsc.gov.uk");

            TlsRptSchedulerState state = await _dao.Get("ncsc.gov.uk");

            Assert.AreEqual(state.Id, "ncsc.gov.uk");
        }

        [Test]
        public async Task ItShouldSaveTheStateIfItDoesNotExist()
        {
            await _dao.Save(new TlsRptSchedulerState("ncsc.gov.uk"));

            await _dao.Get("ncsc.gov.uk");

            TlsRptSchedulerState state = await _dao.Get("ncsc.gov.uk");

            Assert.AreEqual(state.Id, "ncsc.gov.uk");
        }

        [Test]
        public async Task ItShouldThrowAnExceptionIfTheStateAlreadyExists()
        {
            await Insert("ncsc.gov.uk");

            Assert.ThrowsAsync<InvalidOperationException>(() => _dao.Save(new TlsRptSchedulerState("ncsc.gov.uk")));
        }

        protected override string GetDatabaseName() => "tls_rpt";

        protected override Assembly GetSchemaAssembly() => Assembly.GetAssembly(typeof(Migrator));

        private Task Insert(string domain) =>
            MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO tls_rpt_scheduled_records (id, last_checked) VALUES (@domain, UTC_TIMESTAMP())",
                new MySqlParameter("domain", domain));

        private Task TruncateDatabase() =>
            MySqlHelper.ExecuteNonQueryAsync(ConnectionString, "DELETE FROM tls_rpt_scheduled_records;");
    }
}
