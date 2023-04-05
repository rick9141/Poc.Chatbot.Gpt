using Poc.Chatbot.Gpt.Models.Settings;
using Serilog;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Poc.Chatbot.Gpt.Infrastructure.Helpers
{
    [ExcludeFromCodeCoverage]
    public class RequestHandler : DelegatingHandler
    {
        private readonly ApiSettings _settings;
        private readonly Stopwatch _stopwatch;

        public RequestHandler(ApiSettings settings) : base(new HttpClientHandler())
        {
            _settings = settings;
            _stopwatch = new Stopwatch();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = default(HttpResponseMessage);
            var maxRetries = _settings.RequestMaxRetries;
            var delayBetweenErrors = _settings.DelayBetweenErrors;
            var tries = 0;

            do
            {
                try
                {
                    if (tries > 0)
                    {
                        await Task.Delay(delayBetweenErrors, cancellationToken);
                    }

                    _stopwatch.Restart();
                    response = await base.SendAsync(request, cancellationToken);
                    _stopwatch.Stop();

                    var requestBody = request.Content != null ? await request.Content?.ReadAsStringAsync(cancellationToken) : string.Empty;

                    var responseBody = response.Content != null ? await response.Content?.ReadAsStringAsync(cancellationToken) : string.Empty;

                    if (response.IsSuccessStatusCode)
                    {
                        Log.Information("Chat GPT External API Request | URI: {requestUri} | StatusCode: {statusCode} | "
                                        + "Headers: {@requestHeaders}  | Body: {@requestbody} | "
                                        + "Method: {requestMethod} | ResponseBody: {@responseBody}"
                                        + "Tries: {tries} | ResponseTimeInSeconds: {@responseTimeInSeconds} | RequestId: {@requestId}",
                                        request.RequestUri,
                                        response.StatusCode,
                                        request.Headers,
                                        requestBody,
                                        request.Method.Method,
                                        responseBody,
                                        tries + 1,
                                        _stopwatch.Elapsed.TotalSeconds,
                                        Activity.Current?.Id);
                    }
                    else if (tries + 1 < maxRetries)
                    {
                        Log.Warning("RETRY | Chat GPT External API Request | URI: {requestUri} | StatusCode: {statusCode} | "
                                    + "Headers: {@requestHeaders}  | Body: {@requestbody} | "
                                    + "Method: {requestMethod} | ResponseBody: {@responseBody}"
                                    + "Tries: {tries} | ResponseTimeInSeconds: {@responseTimeInSeconds} | RequestId: {@requestId}",
                                    request.RequestUri,
                                    response.StatusCode,
                                    request.Headers,
                                    requestBody,
                                    request.Method.Method,
                                    responseBody,
                                    tries + 1,
                                    _stopwatch.Elapsed.TotalSeconds,
                                    Activity.Current?.Id);
                    }
                    else
                    {
                        Log.Error("Chat GPT External API Request ERROR | URI: {requestUri} | StatusCode: {statusCode} | "
                                  + "Headers: {@requestHeaders}  | Body: {@requestbody} | "
                                  + "Method: {requestMethod} | ResponseBody: {@responseBody}"
                                  + "Tries: {tries} | ResponseTimeInSeconds: {@responseTimeInSeconds} | RequestId: {@requestId}",
                                    request.RequestUri,
                                    response.StatusCode,
                                    request.Headers,
                                    requestBody,
                                    request.Method.Method,
                                    responseBody,
                                    tries + 1,
                                    _stopwatch.Elapsed.TotalSeconds,
                                    Activity.Current?.Id);
                    }

                }
                catch
                {
                    if (tries == maxRetries - 1)
                    {
                        var requestBody = request.Content != null ? await request.Content?.ReadAsStringAsync(cancellationToken) : string.Empty;

                        var responseBody = response != null && response.Content != null ? await response.Content?.ReadAsStringAsync(cancellationToken) : string.Empty;
                        var statusCode = response != null ? response.StatusCode : HttpStatusCode.ServiceUnavailable;

                        Log.Error("Chat GPT External API ERROR | URI: {requestUri} | StatusCode: {responseStatusCode} | "
                                  + "Headers: {@requestHeaders} | RetriesQuantity: {tries} | Body: {@requestBody} | "
                                  + "Method: {requestMethod} | ResponseBody: {@responseBody}"
                                  + "Tries: {tries} | ResponseTimeInSeconds: {@responseTimeInSeconds} | RequestId: {@requestId}",
                                  request.RequestUri,
                                  statusCode,
                                  request.Headers,
                                  tries,
                                  requestBody,
                                  request.Method,
                                  responseBody,
                                  tries + 1,
                                  _stopwatch.Elapsed.TotalSeconds,
                                  Activity.Current?.Id);

                        throw;
                    }
                }
            } while (response?.IsSuccessStatusCode != true && ++tries < maxRetries);

            return response;
        }
    }
}
