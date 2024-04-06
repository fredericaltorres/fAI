using System;

namespace fAI
{
    public class LeonardoException : Exception
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

    public class ChatGPTException : Exception
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

    public class OpenAIAudioSpeechException : Exception
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

    public class PineconeException : Exception
    {
        public PineconeException(string message)
            : base(message)
        {
        }
        public PineconeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}




