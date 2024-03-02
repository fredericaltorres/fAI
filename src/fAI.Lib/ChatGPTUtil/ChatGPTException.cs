using System;

namespace fAI
{
    internal class LeonardoException : Exception
    {
        public LeonardoException(string message)
            : base(message)
        {
        }
        public LeonardoException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

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

    internal class OpenAIAudioSpeechException : Exception
    {
        public OpenAIAudioSpeechException(string message)
            : base(message)
        {
        }
        public OpenAIAudioSpeechException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}


