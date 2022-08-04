using System.Threading.Tasks;
using MailCheck.Common.Api.Authorisation.Filter;
using MailCheck.Common.Api.Authorisation.Service.Domain;
using MailCheck.Common.Api.Domain;
using MailCheck.TlsRpt.Reports.Api.Domain;
using MailCheck.TlsRpt.Reports.Api.Service;
using Microsoft.AspNetCore.Mvc;

namespace MailCheck.TlsRpt.Reports.Api.Controllers
{
    [Route("/api/tlsrpt-reports")]
    [MailCheckAuthoriseResource(Operation.Read, ResourceType.TlsRptReports, "domain")]
    public class TlsRptReportsController : Controller
    {
        private readonly ITlsRptReportsService _reportsService;

        public TlsRptReportsController(ITlsRptReportsService tlsRptService)
        {
            _reportsService = tlsRptService;
        }

        [HttpGet]
        [Route("{domain}")]
        public async Task<IActionResult> GetTlsRptReportEntity(TlsRptReportsEntityRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            TlsRptReportsEntityResponse stateResponse = await _reportsService.GetTlsRptReportsEntity(request.Domain);

            if (stateResponse == null)
            {
                return new NotFoundObjectResult(new ErrorResponse($"No Reports entity found for {request.Domain}",
                    ErrorStatus.Information));
            }

            return new ObjectResult(stateResponse);
        }
    }
}