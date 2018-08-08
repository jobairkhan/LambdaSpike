using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace EventCallback
{
    public class Function
    {
        int endpointDelayMilliseconds;
        TimeSpan callbackTimeout;

        const string delayedUriFormat = "http://mockbin.org/delay/{0}"; // delay in milliseconds before replying
        const string echoUri = "http://mockbin.org/echo"; // returns the POST data

        public Function()
        {
           
        }

        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            var enpointDelaySeconds = Environment.GetEnvironmentVariable("ENDPOINT_DELAY_SECONDS");
            var timeoutSeconds = Environment.GetEnvironmentVariable("TIMEOUT_SECONDS");

            if (Int32.TryParse(enpointDelaySeconds, out var delay))
            {
                endpointDelayMilliseconds = delay * 1000;
            }

            if (Int32.TryParse(timeoutSeconds, out var timeout))
            {
                callbackTimeout = TimeSpan.FromSeconds(timeout);
            }

            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context )
        {
            context.Logger.LogLine($"Processed message {message.Body}");
            context.Logger.LogLine($"Endpoint Delay (ms) {endpointDelayMilliseconds}");
            context.Logger.LogLine($"Callback Timeout (s) {callbackTimeout.TotalSeconds}");

            var source = new CancellationTokenSource(callbackTimeout);
            var token = source.Token;
            var client = new HttpClient();

            using (var registration = token.Register(() =>
            {
                context.Logger.LogLine($"Cancelling request after {callbackTimeout.TotalSeconds}s");
                client.CancelPendingRequests();
            }))
            {
                var content = new StringContent(message.Body, Encoding.UTF8, "application/json");
                var uri = new Uri(string.Format(delayedUriFormat, endpointDelayMilliseconds));

                HttpResponseMessage response = null;

                try
                {
                    response = await client.PostAsync(uri, content, token);
                }
                catch (OperationCanceledException) 
                {
                    
                }
                
                if (response != null)
                {
                    context.Logger.LogLine($"Endpoint responded with {response.StatusCode}");
                    context.Logger.LogLine($"Endpoint body {response.Content.ReadAsStringAsync().Result}");
                }
            }
        }
    }
}
