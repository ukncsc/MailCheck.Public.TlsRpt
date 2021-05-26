using System.IO;
using System.Linq;
using MailCheck.TlsRpt.Reports.Processor.Domain;
using MimeKit;

namespace MailCheck.TlsRpt.Reports.Processor.Parser
{
    public interface IEmailBodyParser
    {
        TlsRptEmail Parse(byte[] emailBody);
    }

    public class EmailBodyParser : IEmailBodyParser
    {
        private readonly IMimeMessageFactory _mimeMessageFactory;
        private readonly IAttachmentStreamNormaliser _attachmentStreamNormaliser;

        public EmailBodyParser(IMimeMessageFactory mimeMessageFactory,
            IAttachmentStreamNormaliser attachmentStreamNormaliser)
        {
            _mimeMessageFactory = mimeMessageFactory;
            _attachmentStreamNormaliser = attachmentStreamNormaliser;
        }

        public TlsRptEmail Parse(byte[] emailBody)
        {
            using (MemoryStream memoryStream = new MemoryStream(emailBody))
            {
                MimeMessage mimeMessage = _mimeMessageFactory.Create(memoryStream);
                
                TlsRptEmail result = new TlsRptEmail
                {
                    TlsReportDomain = mimeMessage.Headers["TLS-Report-Domain"],
                    TlsReportSubmitter = mimeMessage.Headers["TLS-Report-Submitter"],
                    Attachments = mimeMessage.BodyParts.OfType<MimePart>()
                        .Select(_attachmentStreamNormaliser.Normalise)
                        .Where(x => x.RawContent != null)
                        .ToList()
                };

                return result;
            }
        }
    }
}