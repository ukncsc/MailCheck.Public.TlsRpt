using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.EntityHistory.Config;
using MailCheck.TlsRpt.EntityHistory.Service;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using A = FakeItEasy.A;

namespace MailCheck.TlsRpt.EntityHistory.Test.Service
{
    [TestFixture]
    public class TlsRptRuaServiceTest
    {
        private const string Id = "abc.com";

        private ITlsRptRuaValidator _ruaValidator;
        private ILogger<TlsRptRuaService> _log;
        private TlsRptRuaService _tlsRptRuaService;
        private ITlsRptEntityHistoryConfig _config;
        private IMessageDispatcher _dispatcher;

        [SetUp]
        public void SetUp()
        {
            _log = A.Fake<ILogger<TlsRptRuaService>>();
            _config = A.Fake<ITlsRptEntityHistoryConfig>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _ruaValidator = new TlsRptRuaValidator();
            _tlsRptRuaService = new TlsRptRuaService(_config, _dispatcher, _ruaValidator, _log);
        }

        [Test]
        public void NothingPublishedWithInvalidRua()
        {
            string record = "v=TLSRPT;rua=mailto:test@hello.com";
            _tlsRptRuaService.Process(Id, record);
            A.CallTo(() => _dispatcher.Dispatch(A<RuaVerificationChangeFound>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void PublishedRuaVerificationChangeFoundWithOneValidRua()
        {
            string record = "v=TLSRPTv1;rua=mailto:test1234567@tls-rua.mailcheck.service.ncsc.gov.uk";
            _tlsRptRuaService.Process(Id, record);
            A.CallTo(() => _dispatcher.Dispatch(A<RuaVerificationChangeFound>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void PublishedRuaVerificationChangeFoundWithTwoValidRua()
        {
            string record = "v=TLSRPTv1;rua=mailto:test1234567@tls-rua.mailcheck.service.ncsc.gov.uk,mailto:test1234566@tls-rua.mailcheck.service.ncsc.gov.uk";
            _tlsRptRuaService.Process(Id, record);
            A.CallTo(() => _dispatcher.Dispatch(A<RuaVerificationChangeFound>._, A<string>._)).MustHaveHappenedTwiceExactly();
        }

        [Test]
        public void PublishedOneRuaVerificationChangeFoundWithOneValidRuaButTwoEmails()
        {
            string record = "v=TLSRPTv1;rua=mailto:test1234567@tls-rua.mailcheck.service.ncsc.gov.uk,mailto:tlsrpt@tls-rua.mailcheck.service.ncsc.gov.uk";
            _tlsRptRuaService.Process(Id, record);
            A.CallTo(() => _dispatcher.Dispatch(A<RuaVerificationChangeFound>._, A<string>._)).MustHaveHappenedOnceExactly();
        }
    }
}