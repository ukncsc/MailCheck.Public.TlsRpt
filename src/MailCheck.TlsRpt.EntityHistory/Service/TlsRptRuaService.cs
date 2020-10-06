using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.TlsRpt.EntityHistory.Config;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.EntityHistory.Service
{
    public interface ITlsRptRuaService
    {
        void Process(string id, string record);
    }

    public class TlsRptRuaService : ITlsRptRuaService
    {
        private readonly ITlsRptEntityHistoryConfig _tlsRptEntityHistoryConfig;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ITlsRptRuaValidator _ruaValidator;
        private readonly ILogger<TlsRptRuaService> _log;

        public TlsRptRuaService(ITlsRptEntityHistoryConfig tlsRptEntityHistoryConfig,
            IMessageDispatcher dispatcher, ITlsRptRuaValidator ruaValidator, ILogger<TlsRptRuaService> log)
        {

            _tlsRptEntityHistoryConfig = tlsRptEntityHistoryConfig;
            _dispatcher = dispatcher;
            _ruaValidator = ruaValidator;
            _log = log;
        }

        public void Process(string id, string record)
        {
            RuaResult result = _ruaValidator.Validate(record);

            if (result.Valid)
            {
                result.Tokens.ForEach(x => _dispatcher.Dispatch(new RuaVerificationChangeFound(id, "TlsRpt", x),
                    _tlsRptEntityHistoryConfig.SnsTopicArn));

                _log.LogInformation("Published Rua found for {Id}", id);
            }
            else
            {
                _log.LogInformation("Invalid Rua found for {Id}", id);
            }
        }
    }
}
