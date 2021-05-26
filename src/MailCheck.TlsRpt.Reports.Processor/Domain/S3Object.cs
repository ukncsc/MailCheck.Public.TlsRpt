namespace MailCheck.TlsRpt.Reports.Processor.Domain
{
    public class S3Object
    {
        public S3Object(byte[] content)
        {
            Content = content;
        }

        public byte[] Content { get; }
    }
}
