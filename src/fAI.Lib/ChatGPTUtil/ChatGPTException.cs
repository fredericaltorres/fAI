using System;

namespace fAI
{
    internal class ChatGPTException : Exception
    {
        public ChatGPTException(string message)
            : base(message)
        {
        }
        public ChatGPTException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


