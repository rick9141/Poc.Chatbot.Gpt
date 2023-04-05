using Microsoft.AspNetCore.Mvc;
using Poc.Chatbot.Gpt.Attributes;

namespace Poc.Chatbot.Gpt.Controllers
{
    /// <summary>
    /// API Health check
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// API Health check
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult HealthCheck()
        {
            return Ok();
        }
    }
}
