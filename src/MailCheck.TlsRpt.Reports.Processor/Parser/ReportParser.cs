using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.TlsRpt.Reports.Contracts;
using MailCheck.TlsRpt.Reports.Contracts.Schema.Rfc8460201809;
using MailCheck.TlsRpt.Reports.Processor.Domain;
using Microsoft.Extensions.Logging;

namespace MailCheck.TlsRpt.Reports.Processor.Parser
{
    public interface IReportParser
    {
        ReportInfo Parse(TlsRptEmail tlsRptEmail, string originalUrl);
    }

    public class ReportParser : IReportParser
    {
        private readonly IAttachmentParser _attachmentParser;
        private readonly ILogger<ReportParser> _logger;

        public ReportParser(ILogger<ReportParser> logger, IAttachmentParser attachmentParser)
        {
            _logger = logger;
            _attachmentParser = attachmentParser;
        }

        public ReportInfo Parse(TlsRptEmail tlsRptEmail, string originalUri)
        {
            if (tlsRptEmail.Attachments.Count == 0)
            {
                throw new ApplicationException($"Failed to parse: no attachment found where one was expected.");
            }

            if (tlsRptEmail.Attachments.Count > 1)
            {
                string[] attachmentFilenames = tlsRptEmail.Attachments.Select(attachment => attachment.Filename).ToArray();
                string attachmentFilenamesString = string.Join(", ", attachmentFilenames);

                throw new ApplicationException($"Failed to parse: multiple attachments found where only one was expected. {Environment.NewLine} Attachment filenames: {attachmentFilenamesString}");
            }

            AttachmentInfo attachmentInfo = tlsRptEmail.Attachments[0];

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["EmailAttachmentFileName"] = attachmentInfo.Filename,
            }))
            {
                JsonReport jsonReport = _attachmentParser.Parse(attachmentInfo);
                ReportInfo reportInfo = new ReportInfo(jsonReport, jsonReport != null ? JsonReport.Version : "unknown", originalUri, tlsRptEmail.TlsReportDomain, tlsRptEmail.TlsReportSubmitter);

                _logger.LogInformation($"Successfully processed attachment.");

                return reportInfo;
            }
        }
    }
}