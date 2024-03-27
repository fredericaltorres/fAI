namespace fAI
{
    public interface IOpenAICompletion
    {
        CompletionResponse Create(GPTPrompt p);
    }
}

