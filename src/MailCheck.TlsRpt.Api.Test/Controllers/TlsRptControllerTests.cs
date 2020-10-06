using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Api.Authorisation.Filter;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Api.Config;
using MailCheck.TlsRpt.Api.Controllers;
using MailCheck.TlsRpt.Api.Dao;
using MailCheck.TlsRpt.Api.Domain;
using MailCheck.TlsRpt.Api.Service;
using MailCheck.TlsRpt.Contracts.SharedDomain;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MailCheck.Common.Api.Authorisation.Filter;
using MailCheck.Common.Api.Authorisation.Service;
using MailCheck.Common.Api.Authorisation.Service.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Api.Test.Controllers
{
    [TestFixture]
    public class TlsRptControllerTests
    {
        private TlsRptController _sut;
        private ITlsRptService _TlsRptService;

        [SetUp]
        public void SetUp()
        {
            _TlsRptService = A.Fake<ITlsRptService>();
            _sut = new TlsRptController(A.Fake<ITlsRptApiDao>(), A.Fake<IMessagePublisher>(), A.Fake<ITlsRptApiConfig>(), _TlsRptService);
        }

        [Test]
        public async Task ItShouldReturnNotFoundWhenThereIsNoTlsRptState()
        {
            A.CallTo(() => _TlsRptService.GetTlsRptForDomain(A<string>._))
                .Returns(Task.FromResult<TlsRptInfoResponse>(null));

            IActionResult response = await _sut.GetTlsRpt(new TlsRptDomainRequest { Domain = "ncsc.gov.uk" });

            Assert.That(response, Is.TypeOf(typeof(NotFoundObjectResult)));
        }

        [Test]
        public async Task ItShouldReturnTheFirstResultWhenTheTlsRptStateExists()
        {
            TlsRptInfoResponse state = new TlsRptInfoResponse("ncsc.gov.uk", TlsRptState.Created);

            A.CallTo(() => _TlsRptService.GetTlsRptForDomain(A<string>._))
                .Returns(Task.FromResult(state));

            ObjectResult response = (ObjectResult)await _sut.GetTlsRpt(new TlsRptDomainRequest { Domain = "ncsc.gov.uk" });

            Assert.AreSame(response.Value, state);
        }

        [Test]
        public void AllEndpointsHaveAuthorisation()
        {
            IEnumerable<MethodInfo> controllerMethods = _sut.GetType().GetMethods().Where(x => x.DeclaringType == typeof(TlsRptController));

            foreach (MethodInfo methodInfo in controllerMethods)
            {
                Assert.That(methodInfo.CustomAttributes.Any(x => x.AttributeType == typeof(MailCheckAuthoriseResourceAttribute) || x.AttributeType == typeof(MailCheckAuthoriseRoleAttribute)));
            }
        }
    }
}
