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
        public void EnglishTextImprove_OpenAI()
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

        [Fact()]
        [TestBeforeAfter]
        public void EnglishTextImprove_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            var text = @"
hi Alice I wanted to let you know that I review the previous email about your car insurance policy I read the proposal I approved we can move on 
";

            var client = new GenericAI();
            var result = client.Completions.TextImprovement(text: text, language: "English",  model: "gemini-2.5-flash");
            result = client.Completions.TextImprovement(text: text, language: "English", model: "gemini-3-flash-preview");
            result = client.Completions.TextImprovement(text: text, language: "English", model: "gpt-5.2");
        }
    }
}