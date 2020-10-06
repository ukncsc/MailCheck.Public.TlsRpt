using System.Threading.Tasks;
using MailCheck.Common.Api.Authorisation.Filter;
using MailCheck.Common.Api.Authorisation.Service.Domain;
using MailCheck.Common.Api.Domain;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Api.Config;
using MailCheck.TlsRpt.Api.Dao;
using MailCheck.TlsRpt.Api.Domain;
using MailCheck.TlsRpt.Api.Service;
using MailCheck.TlsRpt.Contracts.Scheduler;
using Microsoft.AspNetCore.Mvc;

namespace MailCheck.TlsRpt.Api.Controllers
{
    [Route("/api/tlsrpt")]
    public class TlsRptController : Controller
    {
        private readonly ITlsRptApiDao _dao;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ITlsRptApiConfig _config;
        private readonly ITlsRptService _tlsRptService;

        public TlsRptController(ITlsRptApiDao dao,
            IMessagePublisher messagePublisher,
            ITlsRptApiConfig config,
            ITlsRptService tlsRptService)
        {
            _dao = dao;
            _messagePublisher = messagePublisher;
            _config = config;
            _tlsRptService = tlsRptService;
        }

        [HttpGet("{domain}/recheck")]
        [MailCheckAuthoriseResource(Operation.Update, ResourceType.TlsRpt, "domain")]
        public async Task<IActionResult> RecheckTlsRpt(TlsRptDomainRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            await _messagePublisher.Publish(new TlsRptRecordExpired(request.Domain), _config.SnsTopicArn);

            return new OkObjectResult("{}");
        }

        [HttpGet("{domain}")]
        [MailCheckAuthoriseRole(Role.Standard)]
        public async Task<IActionResult> GetTlsRpt(TlsRptDomainRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            TlsRptInfoResponse response = await _tlsRptService.GetTlsRptForDomain(request.Domain);

            if (response == null)
            {
                return new NotFoundObjectResult(new ErrorResponse($"No TLS/RPT found for {request.Domain}",
                    ErrorStatus.Information));
            }

            return new ObjectResult(response);
        }

        [HttpPost]
        [Route("domains")]
        [MailCheckAuthoriseRole(Role.Standard)]
        public async Task<IActionResult> GetTlsRptStates([FromBody] TlsRptInfoListRequest request)
        {
            return new ObjectResult(await _dao.GetTlsRptForDomains(request.HostNames));
        }

        [HttpGet]
        [Route("history/{domain}")]
        [MailCheckAuthoriseResource(Operation.Read, ResourceType.TlsRptHistory, "domain")]
        public async Task<IActionResult> GetTlsRptHistory(TlsRptDomainRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            string history = await _dao.GetTlsRptHistory(request.Domain);

            if (history == null)
            {
                return new NotFoundObjectResult(new ErrorResponse($"No TLS/RPT History found for {request.Domain}",
                    ErrorStatus.Information));
            }

            return Content(history, "application/json");
        }
    }
}