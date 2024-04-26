using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.OpenAICompletions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Newtonsoft.Json;
using DynamicSugar;
using static System.Net.Mime.MediaTypeNames;

namespace fAI.Tests
{

    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class AnthropicCompletionsChat : OpenAiCompletionsBase
    {
        public AnthropicCompletionsChat()
        {
            OpenAI.TraceOn = true;
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_JsonMode_WorldCup()
        {
            var p = new Anthropic_Prompt_Claude_3_Opus()
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage { Role =  MessageRole.user, Content = "You are a helpful assistant designed to output JSON." },
                    new GPTMessage { Role =  MessageRole.assistant,   Content = "Who won the soccer world cup in 1998?" }
                }
            };
            OpenAiCompletionsChatMultiImplementation.Virtual_Completion_JsonMode_WorldCup(new Anthropic().Completions, p, "winning_team");
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_JsonMode_WorlaaadCup()
        {
            var p = new Anthropic_Prompt_Claude_3_Opus()
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage { Role =  MessageRole.user, Content = "What is latin for Ant? (A) Apoidea, (B) Rhopalocera, (C) Formicidae" },
                    new GPTMessage { Role =  MessageRole.assistant,   Content = "The answer is (" }
                }
            };
            OpenAiCompletionsChatMultiImplementation.Virtual_Completion_JsonMode_WorldCup(new Anthropic().Completions, p, "winning_team");
        }
    }
}


