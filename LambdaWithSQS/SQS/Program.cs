/*******************************************************************************
* Copyright 2009-2018 Amazon.com, Inc. or its affiliates. All Rights Reserved.
* 
* Licensed under the Apache License, Version 2.0 (the "License"). You may
* not use this file except in compliance with the License. A copy of the
* License is located at
* 
* http://aws.amazon.com/apache2.0/
* 
* or in the "license" file accompanying this file. This file is
* distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
* KIND, either express or implied. See the License for the specific
* language governing permissions and limitations under the License.
*******************************************************************************/

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace SQS
{
    class Program
    {
        private static int _messageCount;
        private static int _batchId;
        private const string QueueName = "BatchQ";

        public static void Main(string[] args)
        {
            Task.WaitAll(Main());
        }

        static async Task Main()
        {
            using (var sqs = new AmazonSQSClient())
            {

                try
                {
                    var qUrl = await CreateNewQueue(sqs);

                    Console.WriteLine("===========================================");
                    Console.WriteLine("Sending Batch Messages to {0}", qUrl);
                    Console.WriteLine("===========================================\n");
                    for (var i = 0; i < 10; i++)
                    {
                        _batchId = i;
                        await SendBatchMessage(sqs, qUrl, 10);
                    }
                }
                catch (AmazonSQSException ex)
                {
                    Console.WriteLine("Caught Exception: " + ex.Message);
                    Console.WriteLine("Response Status Code: " + ex.StatusCode);
                    Console.WriteLine("Error Code: " + ex.ErrorCode);
                    Console.WriteLine("Error Type: " + ex.ErrorType);
                    Console.WriteLine("Request ID: " + ex.RequestId);
                }
            }

            Console.WriteLine("Total sent {0} messages", _messageCount);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static async Task SendBatchMessage(IAmazonSQS amazonSqsClient, string queueUrl, int numberOfMessage = 5)
        {
            var entry1 = CreateNewEntry($"Entry1-{_batchId}", "John Doe", "123 Main St.");
            var entry2 = CreateNewEntry($"Entry2-{_batchId}", "Jane Doe", "Any City, United States");
            var entry3 = CreateNewEntry($"Entry3-{_batchId}", "Richard Doe", "789 East Blvd.");
            //var entry4 = CreateNewEntry($"Entry4-{_batchId}", "Error: Poison Message", "Error Message");
            var entry4 = CreateNewEntry($"Entry4-{_batchId}", "Good Message", "Good Message");
            var entry5 = CreateNewEntry($"Entry5-{_batchId}", "Number Five", "<Empty>");

            var request = new SendMessageBatchRequest
            {
                Entries = new List<SendMessageBatchRequestEntry>() { entry1, entry2, entry3, entry4, entry5 },
                QueueUrl = queueUrl
            };

            for (int i = 0; i < numberOfMessage - 5; i++)
            {
                request.Entries.Add(CreateNewEntry($"Message{i}-{_batchId}", $"Tom {i}", "Don't know", 14000));
            }


            var response = await amazonSqsClient.SendMessageBatchAsync(request, CancellationToken.None);

            if (response.Successful.Count > 0)
            {
                Console.WriteLine("Successfully sent:");

                foreach (var success in response.Successful)
                {
                    Console.WriteLine("  For ID: '" + success.Id + "':");
                    Console.WriteLine("    Message ID = " + success.MessageId);
                    Console.WriteLine("    MD5 of message attributes = " +
                      success.MD5OfMessageAttributes);
                    Console.WriteLine("    MD5 of message body = " +
                      success.MD5OfMessageBody);
                }

                Console.WriteLine("Successfully sent {0} messages with batch id ({1})", response.Successful.Count, _batchId);
                _messageCount += response.Successful.Count;
            }

            if (response.Failed.Count > 0)
            {
                Console.WriteLine("Failed to be sent:");

                foreach (var fail in response.Failed)
                {
                    Console.WriteLine("  For ID '" + fail.Id + "':");
                    Console.WriteLine("    Code = " + fail.Code);
                    Console.WriteLine("    Message = " + fail.Message);
                    Console.WriteLine("    Sender's fault? = " +
                      fail.SenderFault);
                }
            }

        }


        private static SendMessageBatchRequestEntry CreateNewEntry(string entryName, string name, string address, int waitingTime = 0)
        {
            return new SendMessageBatchRequestEntry
            {
                DelaySeconds = 0,
                Id = entryName,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                      "name", new MessageAttributeValue
                        { DataType = "String", StringValue = name }
                    },
                    {
                      "delay", new MessageAttributeValue
                        { DataType = "String", StringValue = waitingTime.ToString() }
                    },
                    {
                      "address", new MessageAttributeValue
                        { DataType = "String", StringValue = address }
                    },
                    {
                      "country", new MessageAttributeValue
                        { DataType = "String", StringValue = "Any Town, United Kingdom" }
                    }
                 },
                MessageBody = $"{name} customer information. {entryName}"
            };
        }

        private static async Task<string> CreateNewQueue(AmazonSQSClient sqs)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("Getting Started with Amazon SQS");
            Console.WriteLine("===========================================\n");
            var myQueueUrl = CreateQueue(sqs);
            await SendMessageToTheQueue(sqs, myQueueUrl);
            //ReceiveMessage(sqs, myQueueUrl);
            return myQueueUrl;
        }



        private static async Task SendMessageToTheQueue(AmazonSQSClient sqs, string myQueueUrl)
        {
            //Sending a message
            Console.WriteLine("Sending a message to MyQueue.\n");
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = myQueueUrl, //URL from initial queue creation
                MessageBody = "This is my message text.",
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {"singleMessage", new MessageAttributeValue{DataType = "Number", StringValue = "1"}}
                }
            };
            await sqs.SendMessageAsync(sendMessageRequest);
            _messageCount++;
        }

        private static string CreateQueue(AmazonSQSClient sqs)
        {
            //Creating a queue
            Console.WriteLine("Create a queue called MyQueue.\n");
            var sqsRequest = new CreateQueueRequest { QueueName = QueueName };
            var createQueueResponse = sqs.CreateQueue(sqsRequest);
            string myQueueUrl = createQueueResponse.QueueUrl;

            //Confirming the queue exists
            var listQueuesRequest = new ListQueuesRequest();
            var listQueuesResponse = sqs.ListQueues(listQueuesRequest);

            Console.WriteLine("Printing list of Amazon SQS queues.\n");
            if (listQueuesResponse.QueueUrls != null)
            {
                foreach (String queueUrl in listQueuesResponse.QueueUrls)
                {
                    Console.WriteLine("  QueueUrl: {0}", queueUrl);
                }
            }
            Console.WriteLine();
            return myQueueUrl;
        }
        private static void ReceiveMessage(AmazonSQSClient sqs, string myQueueUrl)
        {
            //Receiving a message
            var receiveMessageRequest = new ReceiveMessageRequest { QueueUrl = myQueueUrl };
            var receiveMessageResponse = sqs.ReceiveMessage(receiveMessageRequest);
            if (receiveMessageResponse.Messages != null)
            {
                Console.WriteLine("Printing received message.\n");
                foreach (var message in receiveMessageResponse.Messages)
                {
                    Console.WriteLine("  Message");
                    if (!string.IsNullOrEmpty(message.MessageId))
                    {
                        Console.WriteLine("    MessageId: {0}", message.MessageId);
                    }
                    if (!string.IsNullOrEmpty(message.ReceiptHandle))
                    {
                        Console.WriteLine("    ReceiptHandle: {0}", message.ReceiptHandle);
                    }
                    if (!string.IsNullOrEmpty(message.MD5OfBody))
                    {
                        Console.WriteLine("    MD5OfBody: {0}", message.MD5OfBody);
                    }
                    if (!string.IsNullOrEmpty(message.Body))
                    {
                        Console.WriteLine("    Body: {0}", message.Body);
                    }

                    foreach (string attributeKey in message.Attributes.Keys)
                    {
                        Console.WriteLine("  Attribute");
                        Console.WriteLine("    Name: {0}", attributeKey);
                        var value = message.Attributes[attributeKey];
                        Console.WriteLine("    Value: {0}", string.IsNullOrEmpty(value) ? "(no value)" : value);
                    }
                }

                var messageReceiptHandle = receiveMessageResponse.Messages[0].ReceiptHandle;

                //Deleting a message
                Console.WriteLine("Deleting the message.\n");
                var deleteRequest = new DeleteMessageRequest { QueueUrl = myQueueUrl, ReceiptHandle = messageReceiptHandle };
                sqs.DeleteMessage(deleteRequest);
            }
        }
    }
}