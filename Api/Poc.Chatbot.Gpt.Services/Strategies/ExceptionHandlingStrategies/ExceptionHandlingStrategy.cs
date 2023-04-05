using Microsoft.AspNetCore.Http;

namespace Poc.Chatbot.Gpt.Services.Strategies.ExceptionHandlingStrategies
{
    public abstract class ExceptionHandlingStrategy
    {
        public abstract Task<HttpContext> HandleAsync(HttpContext context, Exception exception);
    }
}
