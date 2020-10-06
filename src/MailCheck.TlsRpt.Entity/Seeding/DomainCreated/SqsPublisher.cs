using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.TlsRpt.Entity.Seeding.DomainCreated
{
    internal interface ISqsPublisher
    {
        Task Publish(List<Message> message, string queueUrl);
    }

    internal class SqsPublisher : ISqsPublisher
    {
        private const string Type = "Type";
        private const string Version = "Version";

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly IAmazonSQS _sqsClient;

        public SqsPublisher(IAmazonSQS sqsClient)
        {
            _sqsClient = sqsClient;
        }

        public async Task Publish(List<Message> messages, string queueUrl)
        {
            List<SendMessageBatchRequestEntry> batchRequestEntries = messages.Select(_ => new SendMessageBatchRequestEntry
            {
                Id = Guid.NewGuid().ToString(),
                MessageBody = JsonConvert.SerializeObject(_ , _serializerSettings),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                    {
                        {
                            Type, new MessageAttributeValue
                            {
                                StringValue = _.GetType().Name,
                                DataType = "String"
                            }
                        },
                        {
                            Version, new MessageAttributeValue
                            {
                                StringValue = GetVersion(_.GetType().GetTypeInfo().Assembly.GetName().Version),
                                DataType = "String"
                            }
                        }
                    }

            }).ToList();

            SendMessageBatchRequest sendMessageBatchRequest = new SendMessageBatchRequest(queueUrl, batchRequestEntries);

            await _sqsClient.SendMessageBatchAsync(sendMessageBatchRequest);
        }

        private string GetVersion(Version version)
        {
            return $"{version.Major}.{version.Minor}.{version.Revision}";
        }
    }
}