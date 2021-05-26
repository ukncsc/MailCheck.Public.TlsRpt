using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.SQSEvents;
using Amazon.S3.Util;
using MailCheck.TlsRpt.Reports.Processor.Domain;
using Newtonsoft.Json;

namespace MailCheck.TlsRpt.Reports.Processor.S3
{
    public interface IS3SourceInfoFactory
    {
        List<S3SourceInfo> Create(SQSEvent sqsEvent, string requestId);
    }

    public class S3SourceInfoFactory : IS3SourceInfoFactory
    {
        public List<S3SourceInfo> Create(SQSEvent sqsEvent, string requestId)
        {
            IEnumerable<Tuple<S3EventNotification.S3EventNotificationRecord, string>> eventNotificationRecords = sqsEvent.Records.SelectMany(ConvertMessageToS3Notifications);

            List<S3SourceInfo> sourceInfos = eventNotificationRecords.Select(_ => new S3SourceInfo(_.Item1.S3.Bucket.Name, _.Item1.S3.Object.Key, _.Item1.S3.Object.Size, _.Item2, requestId)).ToList();

            return sourceInfos;
        }

        private List<Tuple<S3EventNotification.S3EventNotificationRecord, string>> ConvertMessageToS3Notifications(SQSEvent.SQSMessage sqsMessage)
        {
            S3EventNotification notification = S3EventNotification.ParseJson(sqsMessage.Body);
            return notification.Records.Select(record => Tuple.Create(record, sqsMessage.MessageId)).ToList();
        }
    }
}