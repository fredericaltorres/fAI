using System;

namespace fAI
{
    internal class TextToSpeechProviderException : Exception
    {
        public TextToSpeechProviderException(string message)
            : base(message)
        {
        }
        public TextToSpeechProviderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

