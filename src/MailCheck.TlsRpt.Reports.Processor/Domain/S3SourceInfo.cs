namespace MailCheck.TlsRpt.Reports.Processor.Domain
{
    public class S3SourceInfo
    {
        public S3SourceInfo(string bucketName, string objectName, long objectSize, string messageId, string requestId)
        {
            BucketName = bucketName;
            ObjectName = objectName;
            ObjectSize = objectSize;
            MessageId = messageId;
            RequestId = requestId;
        }

        public string BucketName { get; }
        public string ObjectName { get; }
        public long ObjectSize { get; }
        public string MessageId { get; }
        public string RequestId { get; }
    }
}