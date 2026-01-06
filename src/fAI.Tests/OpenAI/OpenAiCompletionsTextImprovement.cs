using DynamicSugar;
using fAI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Xunit;
using static fAI.HumeAISpeech;
using static fAI.OpenAICompletions;
using static fAI.OpenAICompletionsEx;
using static System.Net.Mime.MediaTypeNames;

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
            var expectedWords = DS.List("alice", "insurance", "car");
            var client = new GenericAI();

            foreach (var model in GenericAI.GetModels())
            {
                var result = client.Completions.TextImprovement(text: text, language: "English", model: model);
                Assert.True(expectedWords.All(w => result.ToLower().Contains(w)));
            }
        }
    }
}