using Microsoft.AspNetCore.Http;
using RestEase;
using System.Diagnostics.CodeAnalysis;

namespace Poc.Chatbot.Gpt.Services.Strategies.ExceptionHandlingStrategies
{
    [ExcludeFromCodeCoverage]
    public class ApiExceptionHandlingStrategy : ExceptionHandlingStrategy
    {

        public override async Task<HttpContext> HandleAsync(HttpContext context, Exception exception)
        {
            var apiException = exception as ApiException;
            context.Response.StatusCode = (int)apiException!.StatusCode;

            return await Task.FromResult(context);
        }
    }
}
