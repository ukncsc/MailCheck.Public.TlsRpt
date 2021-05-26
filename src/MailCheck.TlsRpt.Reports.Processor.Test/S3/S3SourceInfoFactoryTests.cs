using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.SQSEvents;
using MailCheck.TlsRpt.Reports.Processor.Domain;
using MailCheck.TlsRpt.Reports.Processor.S3;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MailCheck.TlsRpt.Reports.Processor.Test.S3
{
    [TestFixture]
    public class S3SourceInfoFactoryTests
    {
        private S3SourceInfoFactory _s3SourceInfoFactory;

        [SetUp]
        public void SetUp()
        {
            _s3SourceInfoFactory = new S3SourceInfoFactory();
        }

        [Test]
        public void CreateMapsRecordToSingleS3SourceInfo()
        {
            SQSEvent sqsEvent = new SQSEvent { Records = new List<SQSEvent.SQSMessage> { CreateSqsMessage(1) } };

            List<S3SourceInfo> result = _s3SourceInfoFactory.Create(sqsEvent, "testRequestId");

            Assert.AreEqual(result[0].RequestId, "testRequestId");
            Assert.AreEqual(result[0].MessageId, "testMessageId");
            Assert.AreEqual(result[0].BucketName, "testBucketName0");
            Assert.AreEqual(result[0].ObjectName, "testKey0");
            Assert.AreEqual(result[0].ObjectSize, 1234);
        }

        [Test]
        public void CreateMapsRecordsToMultipleS3SourceInfos()
        {
            SQSEvent sqsEvent = new SQSEvent { Records = new List<SQSEvent.SQSMessage> { CreateSqsMessage(2) } };

            List<S3SourceInfo> result = _s3SourceInfoFactory.Create(sqsEvent, "testRequestId");

            Assert.AreEqual(result[0].RequestId, "testRequestId");
            Assert.AreEqual(result[0].MessageId, "testMessageId");
            Assert.AreEqual(result[0].BucketName, "testBucketName0");
            Assert.AreEqual(result[0].ObjectName, "testKey0");
            Assert.AreEqual(result[0].ObjectSize, 1234);

            Assert.AreEqual(result[1].RequestId, "testRequestId");
            Assert.AreEqual(result[1].MessageId, "testMessageId");
            Assert.AreEqual(result[1].BucketName, "testBucketName1");
            Assert.AreEqual(result[1].ObjectName, "testKey1");
            Assert.AreEqual(result[1].ObjectSize, 1234);
        }

        private SQSEvent.SQSMessage CreateSqsMessage(int recordCount)
        {
            return new SQSEvent.SQSMessage
            {
                MessageId = $"testMessageId",
                Body = JsonConvert.SerializeObject(new
                {
                    Records = Enumerable.Range(0, recordCount).Select(x => new
                    {
                        s3 = new
                        {
                            bucket = new { name = $"testBucketName{x}" },
                            @object = new { key = $"testKey{x}", size = 1234 }
                        }
                    }).ToArray()
                })
            };
        }
    }
}
