using System;

//
//               E X P E R I M E N T A L
//

namespace Brainshark.Cognitive.Library.ChatGPT
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


