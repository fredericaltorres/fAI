using System;

namespace fAI
{
    public class SpeechToTextException : Exception
    {
        public SpeechToTextException(string message)
            : base(message)
        {
        }
        public SpeechToTextException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
