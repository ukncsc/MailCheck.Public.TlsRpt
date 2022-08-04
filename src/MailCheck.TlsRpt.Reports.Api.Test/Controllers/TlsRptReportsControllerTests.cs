using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.TlsRpt.Reports.Api.Controllers;
using MailCheck.TlsRpt.Reports.Api.Domain;
using MailCheck.TlsRpt.Reports.Api.Service;
using MailCheck.TlsRpt.Reports.Contracts;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Api.Test.Controllers
{
    [TestFixture]
    public class TlsRptReportsControllerTests
    {
        private TlsRptReportsController _reportsController;
        private ITlsRptReportsService _reportService;

        [SetUp]
        public void SetUp()
        {
            _reportService = A.Fake<ITlsRptReportsService>();
            _reportsController = new TlsRptReportsController(_reportService);
        }
        
        [Test]
        public async Task ItShouldReturnNotFoundWhenThereIsNoReportsState()
        {
            A.CallTo(() => _reportService.GetTlsRptReportsEntity(A<string>._))
                .Returns(Task.FromResult<TlsRptReportsEntityResponse>(null));

            IActionResult response =
                await _reportsController.GetTlsRptReportEntity(new TlsRptReportsEntityRequest {Domain = "ncsc.gov.uk"});

            Assert.That(response, Is.TypeOf(typeof(NotFoundObjectResult)));
        }

        [Test]
        public async Task ItShouldReturnTheFirstResultWhenTheEmailSecurityStateExists()
        {
            TlsRptReportsEntityResponse state = new TlsRptReportsEntityResponse()
            {
                Domain = "ncsc.gov.uk",
                AdvisoryMessages = new List<ReportsAdvisoryMessage>(),
                LastUpdated = DateTime.MaxValue
            };

            A.CallTo(() => _reportService.GetTlsRptReportsEntity(A<string>._))
                .Returns(Task.FromResult(state));

            ObjectResult response = (ObjectResult) await _reportsController.GetTlsRptReportEntity(new TlsRptReportsEntityRequest { Domain = "ncsc.gov.uk" });

            Assert.AreSame(response.Value, state);
        }
    }
}
