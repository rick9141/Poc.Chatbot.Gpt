using Newtonsoft.Json;

namespace Poc.Chatbot.Gpt.Models.Exceptions
{
    public class ErrorResponse
    {
        /// <summary>
        /// Error message text
        /// </summary>
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        public ErrorResponse(string errorMessage, string requestId = "")
        {
            Error = errorMessage;
            RequestId = requestId;
        }
    }
}
