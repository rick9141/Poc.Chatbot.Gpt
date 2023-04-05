using Newtonsoft.Json;
using RestEase;
using Poc.Chatbot.Gpt.Models.Exceptions;
using Poc.Chatbot.Gpt.Services.Strategies.ExceptionHandlingStrategies;
using Serilog;
using System.Diagnostics;

namespace Poc.Chatbot.Gpt.Middleware
{
    /// <summary>
    /// Wraps all controller actions with a try-catch latch to avoid code repetition
    /// </summary>
    public class HandleResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Dictionary<Type, ExceptionHandlingStrategy> _exceptionHandling;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="exceptionHandling"></param>
        public HandleResponseMiddleware(RequestDelegate next,
                                       Dictionary<Type, ExceptionHandlingStrategy> exceptionHandling)
        {
            _next = next;
            _exceptionHandling = exceptionHandling;
        }

        /// <summary>
        /// This method is executed for every single operation in this project
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = default;

            try
            {
                if (HasToLog(context))
                {

                    Log.Information("Chat GPT Custom HTTP {RequestMethod} {RequestPath} "
                                    + "{Query} responded {StatusCode} for request body: "
                                    + "{Body} with headers: {Headers} | Document Number: {@documentNumber} | PhoneNumber: {@phoneNumber}"
                                    + "Host {Host} | RequestId: {@requestId}",
                                    context.Request.Method,
                                    context.Request.Path.Value,
                                    context.Request.Query,
                                    context.Response.StatusCode,
                                    requestBody,
                                    context.Request.Headers,
                                    context.Request.Query.FirstOrDefault(q => q.Key.Equals("CpfCnpj")).Value.ToString(),
                                    context.Request.Query.FirstOrDefault(q => q.Key.Equals("NumeroTelefone")).Value.ToString(),
                                    context.Request.Host.Value,
                                    Activity.Current?.Id);
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(requestBody, context, ex);
            }
        }

        private async Task HandleExceptionAsync(string requestBody, HttpContext context, Exception exception)
        {
            if (exception is ApiException apiException)
            {
                exception = new BaseException(apiException.Message, apiException);
            }

            if (exception is BaseException baseException)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = GetStatusCode(baseException);

                Log.Error(exception,
                            "Chat GPT Custom ERROR {serviceMessage} [traceId:{TraceId}]"
                            + "Headers: {@Headers}. Query: {@Query}. "
                            + "Path: {@Path}. | Document Number: {@documentNumber} | PhoneNumber: {@phoneNumber} | "
                            + "Body: {@Body} | RequestId: {@requestId}",
                            exception.Message,
                            context.TraceIdentifier,
                            context.Request.Headers,
                            context.Request.Query,
                            context.Request.Path,
                            context.Request.Query.FirstOrDefault(q => q.Key.Equals("CpfCnpj")).Value.ToString(),
                            context.Request.Query.FirstOrDefault(q => q.Key.Equals("NumeroTelefone")).Value.ToString(),
                            requestBody,
                            Activity.Current?.Id);

                await context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorResponse($"{baseException.Message} | Check log for details", Activity.Current?.Id)));
            }
            else
            {
                if (_exceptionHandling.TryGetValue(exception.GetType(), out var handler))
                {
                    context = await handler.HandleAsync(context, exception);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }

                Log.Error(exception,
                            "Chat GPT Custom ERROR {serviceMessage} [traceId:{TraceId}]"
                            + "Headers: {@Headers}. Query: {@Query}. "
                            + "Path: {@Path}. | Document Number: {@documentNumber} | PhoneNumber: {@phoneNumber} | "
                            + "Body: {@Body} | RequestId: {@requestId}",
                            exception.Message,
                            context.TraceIdentifier,
                            context.Request.Headers,
                            context.Request.Query,
                            context.Request.Path,
                            context.Request.Query.FirstOrDefault(q => q.Key.Equals("CpfCnpj")).Value.ToString(),
                            context.Request.Query.FirstOrDefault(q => q.Key.Equals("NumeroTelefone")).Value.ToString(),
                            requestBody,
                            Activity.Current?.Id);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorResponse($"{exception.Message} | Check log for details", Activity.Current?.Id)));
            }
        }

        private static int GetStatusCode(BaseException baseException) =>
            baseException.StatusCode > 0
                ? baseException.StatusCode
                : StatusCodes.Status400BadRequest;

        private static bool HasToLog(HttpContext context)
        {
            const string SWAGGER_PATH = "/swagger";
            const string INDEX = "index.html";

            var path = context.Request.Path;
            return !path.Value.StartsWith(SWAGGER_PATH, StringComparison.OrdinalIgnoreCase)
                && !path.Value.Contains(INDEX)
                && !path.Value.Equals("/");
        }
    }
}
