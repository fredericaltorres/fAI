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
using static fAI.OpenAICompletionsEx;

namespace fAI.Tests
{
   

    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class OpenAiCompletionsCodeGeneration : OpenAIUnitTestsBase
    {
        public OpenAiCompletionsCodeGeneration()
        {
            OpenAI.TraceOn = true;
        }

        [Fact()]
        [TestBeforeAfter]
        public void PythonCodeGeneration()
        {
            var client = new OpenAI();
            var prompt = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage> {
                    new GPTMessage { Role =  MessageRole.system, Content = "You are a helpful and experienced Python software developer." },
                    new GPTMessage { Role =  MessageRole.user, Content = "Write a simple Python script that prints 'Hello, World!'" }
                },
            };
            var response = client.Completions.Create(prompt);
            Assert.True(response.Success);
            DS.Assert.Words(response.Text, "python & Hello & World");

            var sourceCode = response.SourceCode;
            Assert.Equal("python", sourceCode.Language);
            DS.Assert.Words(sourceCode.Code, "print & Hello & World");
        }
    }
}


