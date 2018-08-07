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
        public async Task TestSQSEventLambdaFunction()
        {
            var sqsMessage = new SQSEvent.SQSMessage
            {
                Body = "foobar"
            };
            sqsMessage.Attributes = new Dictionary<string, string>
            {
                {"attr", "Test"}
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
    }
}
