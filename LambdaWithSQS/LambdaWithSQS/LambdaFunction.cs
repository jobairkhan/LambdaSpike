using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LambdaWithSQS
{
    public class LambdaFunction
    {
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public LambdaFunction()
        {

        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Handler(SQSEvent evnt, ILambdaContext context)
        {
            var recordCount = evnt.Records.Count;
            var processId = Guid.NewGuid();
            foreach (var message in evnt.Records)
            {
                try
                {
                    context.Logger.LogLine($"#{recordCount--}.  Id {processId}");
                    await ProcessLambdaMessageAsync(message, context);
                }
                catch (PoisonMessageException)
                {
                    Console.WriteLine(message.Body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Find Me:{ex}");
                    throw ex;
                }
            }
            Console.WriteLine($"{evnt.Records.Count} message(s) completed");
        }

        private async Task ProcessLambdaMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            var waitingTime = GetWaitingTime(message);
            context.Logger.LogLine($"Waiting time: {waitingTime}ms");
            await Task.Delay(waitingTime);


            if (message.Body.Contains("Error:")) throw new PoisonMessageException(message.Body);

            if (message.MessageAttributes != null)
                context.Logger.LogLine(string.Join(";",
                    message.MessageAttributes.Select(s => $"{s.Key}: {s.Value.StringValue}")));

            context.Logger.LogLine($"Processed message {message.Body}");

            await Task.CompletedTask;
        }

        private static int GetWaitingTime(SQSEvent.SQSMessage message)
        {
            var messageAttributeDelay = message.MessageAttributes?.FirstOrDefault(x => x.Key?.ToLower() == "delay");
            int.TryParse(messageAttributeDelay?.Value?.StringValue, out var waitingTime);
            return waitingTime <= 0 ? 500 : waitingTime;
        }
    }

    internal class PoisonMessageException : Exception
    {
        public PoisonMessageException(string msgBody)
        : base($"Poison message {msgBody}")
        {

        }
    }
}
