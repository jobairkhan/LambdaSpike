using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;

using Amazon.Lambda.TestUtilities;
using Xunit;

namespace LambdaWithSQS.Tests
{
    public class LambdaFunctionTest
    {
        [Fact]
        public async Task TestSqsEventLambdaFunction()
        {
            var sqsMessage = new SQSEvent.SQSMessage
            {
                Body = "foobar"
            };
            sqsMessage.MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>
            {
                {"attr", new SQSEvent.MessageAttribute{DataType = "String", StringValue = "Test"}}
            };
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    sqsMessage
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new LambdaFunction();
            await function.Handler(sqsEvent, context);

            Assert.Contains("attr: Test", logger.Buffer.ToString());
        }     
        
        [Fact]
        public async Task TestSqsEventLambdaFunctionWithDelay()
        {
            var sqsMessage = new SQSEvent.SQSMessage
            {
                Body = "foobar"
            };
            sqsMessage.MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>
            {
                {"delay", new SQSEvent.MessageAttribute{DataType = "String", StringValue = "10"}}
            };
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    sqsMessage
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new LambdaFunction();
            await function.Handler(sqsEvent, context);

            Assert.Contains("Waiting time: 10ms", logger.Buffer.ToString());
        }
    }
}
