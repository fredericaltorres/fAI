using Newtonsoft.Json;

namespace fAI
{
    public class SpeechToTextSentence
    {
        public SpeechToTextSentence(string[] phrases)
        {
            Phrases = phrases;
        }

        [JsonProperty("phrases")]
        public string[] Phrases { get; }
    }
}
