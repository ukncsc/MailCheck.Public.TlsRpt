using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using MailCheck.Common.Messaging;
using MailCheck.TlsRpt.Entity.Seeding.DomainCreated;
using Microsoft.Extensions.CommandLineUtils;

namespace MailCheck.TlsRpt.Entity
{
    public static class LocalEntryPoint
    {
        public static void Main(string[] args)
        {
            CommandLineApplication commandLineApplication = new CommandLineApplication(false);

            commandLineApplication.OnExecute(() =>
            {
                RunLambda().ConfigureAwait(true).GetAwaiter().GetResult();
                return 0;
            });

            commandLineApplication.Command("seed", command =>
            {
                CommandOption connectionString = command.Option("-d", "TLS/RPT Connection String", CommandOptionType.SingleValue);
                CommandOption queueUrl = command.Option("-q", "Queue Url", CommandOptionType.SingleValue);
                
                command.OnExecute(() =>
                {
                    ISeeder seeder = SeederFactory.Create(connectionString.Value(), queueUrl.Value());

                    seeder.SeedDomainCreated().GetAwaiter().GetResult();

                    return 0;
                });

            });

            commandLineApplication.Execute(args);
        }

        private static async Task RunLambda()
        {
            AmazonSQSClient client = new AmazonSQSClient();
            LambdaEntryPoint lambdaEntryPoint = new LambdaEntryPoint();
            string queueUrl = Environment.GetEnvironmentVariable("SqsQueueUrl");

            ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest(queueUrl)
            {
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = 20,
                MessageAttributeNames = new List<string> { "All" },
                AttributeNames = new List<string> { "All" }
            };

            while (true)
            {
                Console.WriteLine($"Polling {queueUrl} for messages...");
                ReceiveMessageResponse receiveMessageResponse = await client.ReceiveMessageAsync(receiveMessageRequest);
                Console.WriteLine($"Received {receiveMessageResponse.Messages.Count} messages from {queueUrl}.");

                if (receiveMessageResponse.Messages.Any())
                {
                    try
                    {
                        Console.WriteLine("Running Lambda...");
                        SQSEvent sqsEvent = receiveMessageResponse.Messages.ToSqsEvent();
                        await lambdaEntryPoint.FunctionHandler(sqsEvent,
                            LambdaContext.NonExpiringLambda);

                        Console.WriteLine("Lambda completed");

                        Console.WriteLine("Deleting messages...");
                        await client.DeleteMessageBatchAsync(queueUrl,
                            sqsEvent.Records.Select(_ => new DeleteMessageBatchRequestEntry
                            {
                                Id = _.MessageId,
                                ReceiptHandle = _.ReceiptHandle
                            }).ToList());

                        Console.WriteLine("Deleted messages.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"An error occured running lambda {e.Message} {Environment.NewLine} {e.StackTrace}");
                    }
                }
            }
        }

        private static SQSEvent ToSqsEvent(this IEnumerable<Message> messages)
        {
            return new SQSEvent
            {
                Records = messages.Select(_ =>
                    new SQSEvent.SQSMessage
                    {
                        MessageAttributes = _.MessageAttributes.ToDictionary(a => a.Key, a =>
                            new SQSEvent.MessageAttribute
                            {
                                StringValue = a.Value.StringValue
                            }),
                        Attributes = _.Attributes,
                        Body = _.Body,
                        MessageId = _.MessageId,
                        ReceiptHandle = _.ReceiptHandle
                    }).ToList()
            };
        }
    }
}
