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
    public class GroqAiCompletionsChat : OpenAIUnitTestsBase
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
                    new GPTMessage { Role =  MessageRole.system, Content = "Text the question in JSON format with a property 'winner'" },
                    new GPTMessage { Role =  MessageRole.user,   Content = "Who won the soccer world cup in 1998?" }
                }
            };

            var response = new Groq().Completions.Create(p);
            Assert.True(response.Success);
            var answer = response.JsonObject["winner"];
            Assert.Equal("France", answer);
        }
    }
}


