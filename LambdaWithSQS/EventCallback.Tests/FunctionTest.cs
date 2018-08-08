using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;
using System;

namespace EventCallback.Tests
{
    public class FunctionTest
    {
       
        [Fact]
        public async Task lambda_calls_a_url_with_a_simple_JSON_payload()
        {
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = "foobar"
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };


            Environment.SetEnvironmentVariable("TIMEOUT_SECONDS", Convert.ToString(60.0 * 5.0));
            Environment.SetEnvironmentVariable("ENDPOINT_DELAY_SECONDS", "1");

            var function = new Function();
            await function.FunctionHandler(sqsEvent, context);

            var buffer = logger.Buffer.ToString();

            Assert.Contains("Processed message foobar", buffer);
            Assert.Contains("Endpoint responded with OK", buffer);
        }

        [Fact]
        public async Task lambda_calls_a_url_which_does_not_respond_before_timeout()
        {
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = "foobar"
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };


            Environment.SetEnvironmentVariable("TIMEOUT_SECONDS", "1");
            Environment.SetEnvironmentVariable("ENDPOINT_DELAY_SECONDS", "10");

            var function = new Function();
            await function.FunctionHandler(sqsEvent, context);

            var buffer = logger.Buffer.ToString();

            Assert.Contains("Processed message foobar", buffer);
            Assert.Contains("Cancelling request after", buffer);
            Assert.DoesNotContain("Endpoint responded with OK", buffer);
        }

        [Fact]
        public async Task lambda_calls_a_url_which_responds_before_timout()
        {
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = "foobar"
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };


            Environment.SetEnvironmentVariable("TIMEOUT_SECONDS", Convert.ToString(60.0 * 5.0));
            Environment.SetEnvironmentVariable("ENDPOINT_DELAY_SECONDS", "1");


            var function = new Function();
            await function.FunctionHandler(sqsEvent, context);

            var buffer = logger.Buffer.ToString();

            Assert.Contains("Processed message foobar", buffer);
            Assert.Contains("Endpoint responded with OK", buffer);
            Assert.DoesNotContain("Cancelling request after", buffer);
        }
    }
}
