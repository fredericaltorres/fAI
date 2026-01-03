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
    public class OpenAiCompletionsTextImprovement : OpenAIUnitTestsBase
    {
        public OpenAiCompletionsTextImprovement()
        {
            OpenAI.TraceOn = true;
        }

        [Fact()]
        [TestBeforeAfter]
        public void EnglishTextImprove()
        {
            var text = @"
- Create a dentist booking demo 
- Redo fLogviewer AI assistant 
- Install Visual Studio 2026";

            var client = new OpenAI(openAiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

            var result = client.CompletionsEx.TextImprovement(text: text, language: "English", model: "gpt-5-mini");// default
            result = client.CompletionsEx.TextImprovement(text: text, language: "English", model: "gpt-5.2");
            result = client.CompletionsEx.TextImprovement(text: text, language: "English", model: "gpt-5-nano");
        }

    }
}