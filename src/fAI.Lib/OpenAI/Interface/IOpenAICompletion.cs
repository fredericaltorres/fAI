namespace fAI
{
    public interface IOpenAICompletion
    {
        AnthropicErrorCompletionResponse Create(GPTPrompt p);
    }
}

