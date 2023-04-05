using RestEase;
using System.Net.Http.Headers;

namespace Poc.Chatbot.Gpt.Infrastructure.Integrations
{
    public interface IChatGptService
    {
        [Header("Authorization")]
        AuthenticationHeaderValue Authorization { get; set; }


    }
}
