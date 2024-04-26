using System.Collections.Generic;
using Xunit;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class OpenAiCompletionsChatMultiImplementation : OpenAiCompletionsBase
    {
        public OpenAiCompletionsChatMultiImplementation()
        {
            OpenAI.TraceOn = true;
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_JsonMode_WorldCup()
        {
            var p = new Prompt_GPT_4()
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage { Role =  MessageRole.system, Content = "You are a helpful assistant designed to output JSON." },
                    new GPTMessage { Role =  MessageRole.user,   Content = "Who won the soccer world cup in 1998?" }
                }
            };
            Virtual_Completion_JsonMode_WorldCup(new OpenAI().Completions, p, "winner");
        }

        internal static void Virtual_Completion_JsonMode_WorldCup(IOpenAICompletion completionsClient, GPTPrompt p, string jsonProperty)
        {
            var response = completionsClient.Create(p);
            Assert.True(response.Success);
            var answer = response.JsonObject[jsonProperty];
            Assert.Equal("France", answer);
        }
    }
}

