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
        private readonly SQSEvent.SQSMessage _sqsMessage;
        private readonly TestLambdaLogger _testLambdaLogger;
        private readonly TestLambdaContext _testLambdaContext;
        private readonly LambdaFunction _targetLambdaFunction;

        public LambdaFunctionTest()
        {
            _sqsMessage = new SQSEvent.SQSMessage
            {
                Body = "foobar"
            };

            _testLambdaLogger = new TestLambdaLogger();
            _testLambdaContext = new TestLambdaContext
            {
                Logger = _testLambdaLogger
            };

            _targetLambdaFunction = new LambdaFunction();
        }

        [Fact]
        public async Task TestSqsEventLambdaFunction()
        {
            var sqsEvent = CreateSqsEventWithAttribute("attr", "Test");

            await _targetLambdaFunction.Handler(sqsEvent, _testLambdaContext);

            Assert.Contains("attr: Test", _testLambdaLogger.Buffer.ToString());
        }

        [Fact]
        public async Task TestSqsEventLambdaFunctionWithDelay()
        {
            var sqsEvent = CreateSqsEventWithAttribute("delay", "10");
            
            await _targetLambdaFunction.Handler(sqsEvent, _testLambdaContext);

            Assert.Contains("Waiting time: 10ms", _testLambdaLogger.Buffer.ToString());
        }

        private SQSEvent CreateSqsEventWithAttribute(string key, string value)
        {
            _sqsMessage.MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>
            {
                {key, new SQSEvent.MessageAttribute{DataType = "String", StringValue = value}}
            };
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    _sqsMessage
                }
            };
            return sqsEvent;
        }
    }
}
