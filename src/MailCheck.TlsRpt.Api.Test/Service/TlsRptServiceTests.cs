using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Api.Config;
using MailCheck.TlsRpt.Api.Dao;
using MailCheck.TlsRpt.Api.Domain;
using MailCheck.TlsRpt.Api.Service;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Api.Test.Service
{
    [TestFixture]
    public class TlsRptServiceTests
    {
        private TlsRptService _tlsRptService;
        private IMessagePublisher _messagePublisher;
        private ITlsRptApiDao _dao;
        private ITlsRptApiConfig _config;

        [SetUp]
        public void SetUp()
        {
            _messagePublisher = A.Fake<IMessagePublisher>();
            _dao = A.Fake<ITlsRptApiDao>();
            _config = A.Fake<ITlsRptApiConfig>();
            _tlsRptService = new TlsRptService(_messagePublisher, _dao, _config);
        }

        [Test]
        public async Task PublishesDomainMissingMessageWhenDomainDoesNotExist()
        {
            A.CallTo(() => _dao.GetTlsRptForDomain("testDomain"))
                .Returns(Task.FromResult<TlsRptInfoResponse>(null));

            TlsRptInfoResponse result = await _tlsRptService.GetTlsRptForDomain("testDomain");

            A.CallTo(() => _messagePublisher.Publish(A<DomainMissing>._, A<string>._))
                .MustHaveHappenedOnceExactly();
            Assert.AreEqual(null, result);
        }

        [Test]
        public async Task DoesNotPublishDomainMissingMessageWhenDomainExists()
        {
            TlsRptInfoResponse tlsRptInfoResponse = new TlsRptInfoResponse("", TlsRptState.Created);
            A.CallTo(() => _dao.GetTlsRptForDomain("testDomain"))
                .Returns(Task.FromResult(tlsRptInfoResponse));

            TlsRptInfoResponse result = await _tlsRptService.GetTlsRptForDomain("testDomain");

            A.CallTo(() => _messagePublisher.Publish(A<DomainMissing>._, A<string>._))
                .MustNotHaveHappened();
            Assert.AreSame(tlsRptInfoResponse, result);

        }
    }
}