using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Api.Domain;
using MailCheck.TlsRpt.Reports.Api.Controllers;
using MailCheck.TlsRpt.Reports.Api.Dao;
using MailCheck.TlsRpt.Reports.Api.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Api.Test.Controllers
{
    [TestFixture]
    public class ReportsSummaryControllerTests
    {
        private ReportsSummaryController _reportsSummaryController;
        private IReportService _reportService;

        [SetUp]
        public void SetUp()
        {
            _reportService = A.Fake<IReportService>();
            _reportsSummaryController = new ReportsSummaryController(_reportService);
        }

        [Test]
        public async Task GetSummaryTitleReturnsResultFromService()
        {
            List<TitleResult> resultFromService = new List<TitleResult>();
            A.CallTo(() => _reportService.GetSummaryTitle("testDomain")).Returns(resultFromService);

            IActionResult result = await _reportsSummaryController.GetSummaryTitle("testDomain");

            Assert.AreSame(resultFromService, ((OkObjectResult)result).Value);
        }

        [TestCase("2-days", 2)]
        [TestCase("14-days", 14)]
        [TestCase("90-days", 90)]
        public async Task GetSummaryBodyReturnsResultFromService(string requestedPeriod, int expectedPeriod)
        {
            List<ProviderFailure> resultFromService = new List<ProviderFailure>();
            A.CallTo(() => _reportService.GetSummaryBody("testDomain", expectedPeriod)).Returns(resultFromService);

            IActionResult result = await _reportsSummaryController.GetSummaryBody(new DomainDateRangeRequest { Domain = "testDomain", Period = requestedPeriod });

            Assert.AreSame(resultFromService, ((OkObjectResult)result).Value);
        }

        [Test]
        public async Task GetSummaryBodyReturnsBadResult()
        {
            _reportsSummaryController.ModelState.AddModelError("testErrorKey", "testErrorValue");

            IActionResult result = await _reportsSummaryController.GetSummaryBody(new DomainDateRangeRequest());

            BadRequestObjectResult badResult = (BadRequestObjectResult)result;
            Assert.AreEqual(400, badResult.StatusCode);
            Assert.AreEqual("testErrorValue", ((ErrorResponse)badResult.Value).Message);
        }

        [Test]
        public void GetSummaryBodyThrows()
        {
            ArgumentException argumentException = Assert.ThrowsAsync<ArgumentException>(() => _reportsSummaryController.GetSummaryBody(new DomainDateRangeRequest { Domain = "testDomain", Period = "bad-value" }));

            Assert.AreEqual("period", argumentException.Message);
        }

        [TestCase("2-days", 2)]
        [TestCase("14-days", 14)]
        [TestCase("90-days", 90)]
        public async Task GetSummaryDetailsReturnsResultFromService(string requestedPeriod, int expectedPeriod)
        {
            List<ReportLine> resultFromService = new List<ReportLine>();
            A.CallTo(() => _reportService.GetSummaryDetails("testDomain", expectedPeriod)).Returns(resultFromService);

            HttpContext httpContext = A.Fake<HttpContext>();
            _reportsSummaryController.ControllerContext = new ControllerContext { HttpContext = httpContext };

            IActionResult result = await _reportsSummaryController.GetSummaryDetails(new DomainDateRangeRequest { Domain = "testDomain", Period = requestedPeriod });

            Assert.AreSame(resultFromService, ((OkObjectResult)result).Value);
            Assert.AreEqual($"attachment; filename=\"TlsReportExport-testDomain-{requestedPeriod}.csv\"", httpContext.Response.Headers[HeaderNames.ContentDisposition][0]);
        }

        [Test]
        public async Task GetSummaryDetailsReturnsBadResult()
        {
            _reportsSummaryController.ModelState.AddModelError("testErrorKey", "testErrorValue");

            IActionResult result = await _reportsSummaryController.GetSummaryDetails(new DomainDateRangeRequest());

            BadRequestObjectResult badResult = (BadRequestObjectResult)result;
            Assert.AreEqual(400, badResult.StatusCode);
            Assert.AreEqual("testErrorValue", ((ErrorResponse)badResult.Value).Message);
        }

        [Test]
        public void GetSummaryDetailsThrows()
        {
            ArgumentException argumentException = Assert.ThrowsAsync<ArgumentException>(() => _reportsSummaryController.GetSummaryDetails(new DomainDateRangeRequest { Domain = "testDomain", Period = "bad-value" }));

            Assert.AreEqual("period", argumentException.Message);
        }
    }
}
