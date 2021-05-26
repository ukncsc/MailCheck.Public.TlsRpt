using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Common.Api.Authorisation.Filter;
using MailCheck.Common.Api.Authorisation.Service.Domain;
using MailCheck.Common.Api.Domain;
using MailCheck.TlsRpt.Reports.Api.Dao;
using MailCheck.TlsRpt.Reports.Api.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace MailCheck.TlsRpt.Reports.Api.Controllers
{
    [Route("/api/tlsrpt-reports/{domain}/summary")]
    [MailCheckAuthoriseResource(Operation.Read, ResourceType.TlsRptReports, "domain")]
    public class ReportsSummaryController : Controller
    {
        private readonly IReportService _tlsRptService;

        public ReportsSummaryController(IReportService tlsRptService)
        {
            _tlsRptService = tlsRptService;
        }

        [HttpGet]
        [Route("title")]
        public async Task<IActionResult> GetSummaryTitle(string domain)
        {
            List<TitleResult> reportSummaryTitle = await _tlsRptService.GetSummaryTitle(domain);
            
            return Ok(reportSummaryTitle);
        }

        [HttpGet]
        [Route("{period}/body")]
        public async Task<IActionResult> GetSummaryBody(DomainDateRangeRequest domainDateRangeRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            List<ProviderFailure> reportSummary = await _tlsRptService.GetSummaryBody(domainDateRangeRequest.Domain, ParsePeriod(domainDateRangeRequest.Period));

            return Ok(reportSummary);
        }

        [HttpGet]
        [Route("{period}/detail")]
        [Produces("text/csv")]
        public async Task<IActionResult> GetSummaryDetails(DomainDateRangeRequest domainDateRangeRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            IEnumerable<ReportLine> details = await _tlsRptService.GetSummaryDetails(domainDateRangeRequest.Domain, ParsePeriod(domainDateRangeRequest.Period));

            Response.Headers[HeaderNames.ContentDisposition] = $"attachment; filename=\"TlsReportExport-{domainDateRangeRequest.Domain}-{domainDateRangeRequest.Period}.csv\"";
            return Ok(details);
        }

        private int ParsePeriod(string period)
        {
            return period == "2-days"
                ? 2
                : period == "14-days"
                    ? 14
                    : period == "90-days"
                        ? 90
                        : throw new ArgumentException(nameof(period));
        }
    }
}