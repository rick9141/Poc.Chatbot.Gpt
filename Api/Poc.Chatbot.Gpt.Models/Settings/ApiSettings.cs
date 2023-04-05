namespace Poc.Chatbot.Gpt.Models.Settings
{
    public class ApiSettings
    {
        /// <summary>
        /// Defines authentication settings to be used in Authorization header
        /// </summary>
        public AuthenticationSettings AuthenticationSettings { get; set; }

        /// <summary>
        /// Sets when API call should throw Timeout Exception
        /// </summary>
        public int DefaultTimeoutMs { get; set; }

        /// <summary>
        /// Sets how many milliseconds API should wait to try to call external API again after error
        /// </summary>
        public int DelayBetweenErrors { get; set; }

        /// <summary>
        /// Sets maximum attempts when RestEase API call fails
        /// </summary>
        public int RequestMaxRetries { get; set; }
    }

    public class AuthenticationSettings
    {
        public string ApiKey { get; set; }
    }
}
