using RestEase;
using Poc.Chatbot.Gpt.Models.Settings;
using Poc.Chatbot.Gpt.Services.Strategies.ExceptionHandlingStrategies;
using Serilog;
using Serilog.Exceptions;

namespace Poc.Chatbot.Gpt
{
    /// <summary>
    /// Program.cs extension
    /// </summary>
    public static class ServicesExtensions
    {
        private const string SETTINGS_SECTION = "Settings";
        private const string APPLICATION_KEY = "Application";
        private const string PROJECT_NAME = "Poc.Chatbot.Gpt";

        /// <summary>
        /// Add injections and other settings
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddSingletons(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection(SETTINGS_SECTION).Get<ApiSettings>();

            services.AddSingleton(settings);

            services.AddInjections();
            services.AddRestClients(settings);

            services.AddSingleton(provider =>
            {
                return new Dictionary<Type, ExceptionHandlingStrategy>
                {
                    { typeof(ApiException), new ApiExceptionHandlingStrategy() },
                    { typeof(NotImplementedException), new NotImplementedExceptionHandlingStrategy() }
                };
            });

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithMachineName()
                .Enrich.WithProperty(APPLICATION_KEY, PROJECT_NAME)
                .Enrich.WithExceptionDetails()
                .CreateLogger();
        }

        private static void AddInjections(this IServiceCollection services)
        {
            //Add your dependency injections here
        }

        private static void AddRestClients(this IServiceCollection services, ApiSettings settings)
        {
            //Add your rest services here
        }
    }
}
