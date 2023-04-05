using Microsoft.AspNetCore.Mvc.Filters;
using Poc.Chatbot.Gpt.Models.Exceptions;
using Poc.Chatbot.Gpt.Models.Settings;

namespace Poc.Chatbot.Gpt.Attributes
{
    /// <summary>
    /// Attribute to add API KEY to Authorization header
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string API_KEY_HEADER_NAME = "Authorization";
        private const string SETTINGS_SECTION = "Settings";

        private string ApiKeyValue { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ApiKeyAttribute()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiKeyValue"></param>
        public ApiKeyAttribute(string apiKeyValue)
        {
            ApiKeyValue = apiKeyValue;
        }

        /// <summary>
        /// Validate API Key
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Response.HasStarted)
            {
                if (string.IsNullOrWhiteSpace(ApiKeyValue))
                {
                    var apiSettings = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>()
                                                                         .GetSection(SETTINGS_SECTION)
                                                                         .Get<ApiSettings>();

                    if (string.IsNullOrWhiteSpace(apiSettings.AuthenticationSettings.ApiKey))
                    {
                        await next();
                        return;
                    }

                    ApiKeyValue = apiSettings.AuthenticationSettings.ApiKey;
                }

                var hasApiKey = context.HttpContext.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var apiKeyVal);

                if (!hasApiKey)
                {
                    throw new BaseException("Authorization header not found", StatusCodes.Status401Unauthorized);
                }

                if (!ApiKeyValue.Equals(apiKeyVal))
                {
                    throw new BaseException("Unauthorized client", StatusCodes.Status401Unauthorized);
                }

                await next();
            }
        }
    }
}
