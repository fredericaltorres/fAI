using Anthropic;
using Anthropic.Models.Messages;

// Reads ANTHROPIC_API_KEY from environment variable automatically
AnthropicClient client = new();

var parameters = new MessageCreateParams()
{
    MaxTokens = 1024,
    Messages =
    [
        new() { Role = Role.User, Content = "What is the capital of France?" }
    ],
    Model = Model.ClaudeOpus4_6
};

var message = await client.Messages.Create(parameters);
//Console.WriteLine(message);
Console.WriteLine(message.Content[0]);
Console.ReadLine();
