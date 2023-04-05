using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace Poc.Chatbot.Gpt.Services.Strategies.ExceptionHandlingStrategies
{
    [ExcludeFromCodeCoverage]
    public class NotImplementedExceptionHandlingStrategy : ExceptionHandlingStrategy
    {
        public override async Task<HttpContext> HandleAsync(HttpContext context, Exception exception)
        {
            context.Response.StatusCode = StatusCodes.Status501NotImplemented;

            return await Task.FromResult(context);
        }
    }
}
