# Spiking Integration of SQS with AWS Lambda 
You can configure incoming messages to trigger a Lambda function. For more information, see Configuring Messages Arriving in an Amazon SQS Queue to Trigger a Lambda Function. Spiking Integration of SQS with AWS Lambda 

## Prerequisits:
- AWS Account
- AWS Visual Studio 2017 SDK extension
- .Net core 2.1

## Projects
1. Lambda function to work with SQSEvent.Messages
1. How to create the timeout mechanism for a long running lambda
1. Console app to send batch message to SQS

## Findings

- As of August 2018 FIFO queues don't support Lambda function triggers. 

- Lambda Batchsize is the largest number of records that will be read from queue at once. 

- With the SQS / Lambda integration, a batch of messages succeeds or fails together. AWS will only delete the messages from the queue if your function returned successfully without any errors

- Lambda service will scale the polling operations up and down based on the number of inflight messages in the Standard Queue

- `async await` is supported with AWS sdk.net. A sample spike project can be found here

## Useful Tips

1. Lambda function timeout has to be lower than the queue’s visibility timeout in order to create the event mapping from SQS to Lambda. 
1. Preventing overloading a database or avoiding rate-limits on a third-party API when processing a large batch of messages. Lambda's concurrency controls are useful
1. A lambda or application is required to move dead letter queue messages to Standard Queue. Message attributes and message attribute values need to be considered
1. SQS Delay Queues can be used to hide message for those the Order is important
1. Lambda function timeout should consider the SQS trigger Batch Size.
1. If you don't know how long it takes to process a message, create a heartbeat for your consumer process: 
  Specify the initial visibility timeout (for example, 2 minutes) 
  and then—as long as your consumer still works on the message—
  keep extending the visibility timeout by 2 minutes every minute.
