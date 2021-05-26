namespace MailCheck.TlsRpt.Reports.Processor.Domain
{
    public class AttachmentInfo
    {
        public static AttachmentInfo EmptyAttachmentInfo = new AttachmentInfo(string.Empty, null);
        public string Filename { get; }
        public byte[] RawContent { get; }

        public AttachmentInfo(string filename, byte[] rawContent)
        {
            Filename = filename;
            RawContent = rawContent;
        }
    }
}