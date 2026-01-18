namespace fAI
{
    public interface IOpenAICompletion
    {
        AnthropicCompletionResponse Create(GPTPrompt p);
    }
}

