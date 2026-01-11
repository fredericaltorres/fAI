namespace fAI
{
    /// <summary>
    /// https://platform.openai.com/docs/guides/text-to-speech
    /// </summary>
    public class  OpenAIAudio
    {
        public string _key { get; }

        public OpenAIAudio(string openAiKey = null)
        {
            _key = openAiKey;
        }

        OpenAISpeech _speech;
        public OpenAISpeech Speech 
        {
            get
            {
                if (_speech == null )
                    _speech = new OpenAISpeech(openAiKey: _key); 
                return _speech;
            }
            set { 
                _speech = value;
            } 
        }

        OpenAITranscriptions _transcriptions;
        public OpenAITranscriptions Transcriptions 
        { 
            get 
            { 
                if (_transcriptions == null)
                    _transcriptions = new OpenAITranscriptions(openAiKey: _key);
                return _transcriptions;
            } 
            set { 
                _transcriptions = value;
            }
        } 
        
    }

    public class OpenAIChat
    {
        public OpenAICompletions Completions { get; private set; } = new OpenAICompletions();
    }

    public class HumeAIAudio
    {
        public HumeAISpeech Speech { get; private set; }
        public string _apiKey { get; }

        public HumeAIAudio(string apiKey)
        {
            _apiKey = apiKey;
            Speech = new HumeAISpeech(apiKey: _apiKey);
        }
    }

}

