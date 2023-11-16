namespace fAI
{
    public class OpenAI
    {
        public OpenAIAudio Audio { get; private set; } = new OpenAIAudio();
        public OpenAICompletions Completions { get; private set; } = new OpenAICompletions();
    }
}

