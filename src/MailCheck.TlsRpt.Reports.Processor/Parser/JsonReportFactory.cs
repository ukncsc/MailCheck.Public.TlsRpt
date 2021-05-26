using MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809;
using MailCheck.TlsRpt.Reports.Processor.Domain;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MailCheck.TlsRpt.Reports.Processor.Parser
{
    public interface IAttachmentParser
    {
        JsonReport Parse(AttachmentInfo attachmentInfo);
    }

    public class AttachmentParser : IAttachmentParser
    {
        private readonly ILogger<AttachmentParser> _logger;

        public AttachmentParser(ILogger<AttachmentParser> logger)
        {
            _logger = logger;
        }
        public JsonReport Parse(AttachmentInfo attachmentInfo)
        {
            string raw = System.Text.Encoding.Default.GetString(attachmentInfo.RawContent);

            bool success = true;
            JsonReport jsonReport = JsonConvert.DeserializeObject<JsonReport>(raw, new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    success = false;
                    _logger.LogWarning($"Unable to deserialise {attachmentInfo.Filename} with error {args.ErrorContext.Error.Message}");
                    args.ErrorContext.Handled = true;
                }
            });

            return success ? jsonReport : null;
        }
    }
}