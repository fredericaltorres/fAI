namespace fAI
{
    /// <summary>
    /// https://platform.openai.com/docs/guides/text-to-speech
    /// </summary>
    public class  OpenAIAudio
    {
        public OpenAISpeech Speech { get; private set; } = new OpenAISpeech();
        public OpenAITranscriptions Transcriptions { get; private set; } = new OpenAITranscriptions();
    }

    public class OpenAIChat
    {
        public OpenAICompletions Completions { get; private set; } = new OpenAICompletions();
    }

    public class HumeAIAudio
    {
        public HumeAISpeech Speech { get; private set; } = new HumeAISpeech();
    }

}

