namespace fAI
{
    /// <summary>
    /// https://platform.openai.com/docs/guides/text-to-speech
    /// https://platform.openai.com/docs/guides/speech-to-text
    /// https://platform.openai.com/docs/guides/text-to-speech#custom-voices
    /// I am the owner of this voice and I consent to OpenAI using this voice to create a synthetic voice model.
    /// 
    /// C:\DVT\fAI\src\fAI.Lib\OpenAI\Audio\VoiceSamples\Fred OpenAI Sample.wav
    /// curl.exe https://api.openai.com/v1/audio/voice_consents -X POST -H "Authorization: Bearer xxxx" -F "name=test_consent" -F "language=en" -F "recording=@C:\DVT\fAI\src\fAI.Lib\OpenAI\Audio\VoiceSamples\Fred OpenAI Sample.wav;type=audio/x-wav"
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

