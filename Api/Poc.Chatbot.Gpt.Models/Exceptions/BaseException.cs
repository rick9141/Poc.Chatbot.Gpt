using System.Runtime.Serialization;

namespace Poc.Chatbot.Gpt.Models.Exceptions
{
    [Serializable]
    public class BaseException : Exception
    {
        public int StatusCode;

        public BaseException()
        {
        }

        public BaseException(string message) : base(message)
        {
        }

        public BaseException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public BaseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
