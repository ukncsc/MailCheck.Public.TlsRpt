using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.Api.Domain;
using MailCheck.TlsRpt.Api.Config;
using MailCheck.TlsRpt.Api.Dao;

namespace MailCheck.TlsRpt.Api.Service
{
    public interface ITlsRptService
    {
        Task<TlsRptInfoResponse> GetTlsRptForDomain(string requestDomain);
    }

    public class TlsRptService : ITlsRptService
    {
        private readonly ITlsRptApiDao _dao;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ITlsRptApiConfig _config;

        public TlsRptService(IMessagePublisher messagePublisher, ITlsRptApiDao dao, ITlsRptApiConfig config)
        {
            _messagePublisher = messagePublisher;
            _dao = dao;
            _config = config;
        }

        public async Task<TlsRptInfoResponse> GetTlsRptForDomain(string requestDomain)
        {
            TlsRptInfoResponse response = await _dao.GetTlsRptForDomain(requestDomain);

            if (response is null)
            {
                await _messagePublisher.Publish(new DomainMissing(requestDomain), _config.MicroserviceOutputSnsTopicArn);
            }

            return response;
        }
    }
}
