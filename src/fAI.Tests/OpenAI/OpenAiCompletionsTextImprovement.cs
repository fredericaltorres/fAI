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

        [Fact()]
        [TestBeforeAfter]
        public void SummarizationResult()
        {
            Assert.Equal(4, GenericAICompletions.SummarizationResult.CountWords("This is a test."));
            Assert.Equal(4, GenericAICompletions.SummarizationResult.CountWords("This was, a test."));
            Assert.Equal(4, GenericAICompletions.SummarizationResult.CountWords(@"This 
                                                                                  was 
                                                                                  a test."));
        }

        [Fact()]
        [TestBeforeAfter]
        public void EnglishTextSummarize_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            var text = @"
A groundbreaking study published in Cell approximately seven years ago by researchers in Israel, 
titled 'Personalized Nutrition by Prediction of Glycemic Responses', 
generated considerable interest. 
This research effectively demonstrated that individuals can exhibit significantly different glycemic responses 
to the same food, 
even something as simple as a handful of blueberries.
This finding challenges the conventional understanding of the glycemic index, 
which posits a predictable glucose rise based on the quantity of food and its glucose content. 

This is important because sustained glycemic variability over time can negatively impact our health. 
It is beneficial to select or balance foods in a way that promotes greater stability in blood sugar levels.

Therefore, understanding your individual glycemic response to various foods is crucial. Furthermore, 
adopting lifestyle strategies such as improving sleep quality, engaging in post-meal walks, 
incorporating resistance training, and utilizing cold exposure techniques can also contribute to better 
glycemic control and overall well-being.
";
            var expectedWords = DS.List("alice", "insurance", "car");
            var client = new GenericAI();

            foreach (var model in GenericAI.GetModels())
            {
                var result = client.Completions.Summarize(text: text, language: "English", model: model);
                //Assert.True(expectedWords.All(w => result.Summary.ToLower().Contains(w))); // Uncommenting ASSERT statement
                HttpBase.Trace($"[SUMMARIZATION] model: {model}, %: {result.PercentageSummzarized}, TextWordCount: {result.TextWordCount}, SummaryWordCount: {result.SummaryWordCount}, Duration: {result.Duration:0.0}", this);
                //Assert.Equal(100, result.TextWordCount);
                //Assert.Equal(100, result.SummaryWordCount);
            }
        }
    }
}