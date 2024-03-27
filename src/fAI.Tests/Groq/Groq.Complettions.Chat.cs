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
    public class GroqAiCompletionsChat : OpenAiCompletionsBase
    {
        public GroqAiCompletionsChat()
        {
            OpenAI.TraceOn = true;
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_JsonMode_WorldCup()
        {
            var p = new Groq_Prompt_Mistral()
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage { Role =  MessageRole.system, Content = "You are a helpful assistant designed to output JSON." },
                    new GPTMessage { Role =  MessageRole.user,   Content = "Who won the soccer world cup in 1998?" }
                }
            };
            OpenAiCompletionsChatMultiImplementation.Virtual_Completion_JsonMode_WorldCup(new Groq().Completions, p, "winning_team");
        }
    }
}


