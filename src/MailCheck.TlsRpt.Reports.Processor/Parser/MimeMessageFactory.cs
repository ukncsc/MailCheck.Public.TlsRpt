using System.IO;
using MimeKit;

namespace MailCheck.TlsRpt.Reports.Processor.Parser
{
    public interface IMimeMessageFactory
    {
        MimeMessage Create(Stream stream);
    }

    public class MimeMessageFactory : IMimeMessageFactory
    {
        public MimeMessage Create(Stream stream)
        {
            MimeParser parser = new MimeParser(stream, MimeFormat.Entity);
            MimeMessage mimeMessage = parser.ParseMessage();
            return mimeMessage;
        }
    }
}
