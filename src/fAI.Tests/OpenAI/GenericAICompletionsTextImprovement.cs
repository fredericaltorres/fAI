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
    public class GenericAiCompletions_UnitTests : OpenAIUnitTestsBase
    {
        public GenericAiCompletions_UnitTests()
        {
            OpenAI.TraceOn = true;
        }
       
        [Fact()]
        [TestBeforeAfter]
        public void ImproveEnglishText_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            var text = @"
hi Alice I wanted to let you know that I review the previous email about your car insurance policy I read the proposal I approved we can move on 
";
            var expectedWords = DS.List("alice", "insurance", "car");
            foreach (var model in GenericAI.GetModels())
            {
                var client = new GenericAI();
                var result = client.Completions.TextImprovement(text: text, language: "English", model: model);
                Assert.True(expectedWords.All(w => result.Text.ToLower().Contains(w)));
                HttpBase.Trace($"[SUMMARIZATION] model: {model}, Duration: {result.Duration:0.0}, ", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void ImproveEnglishText_GenericAI_InterfaceForOpenAIAndGoogle_ConversationMode()
        {
            var text = @"
hi Alice I wanted to let you know that I review the previous email about your car insurance policy I read the proposal I approved we can move on 
";
            var expectedWords = DS.List("alice", "insurance", "car");
            var models = DS.List("gemini-2.0-flash", "gpt-5-mini");

            foreach (var model in models)
            {
                var client = new GenericAI();

                // Conversation step 1
                var result = client.Completions.TextImprovement(text: text, language: "English", model: model);
                Assert.True(expectedWords.All(w => result.Text.ToLower().Contains(w)));

                Assert.Equal(2, result.Contents.Count); // Query + Response
                Assert.Equal("user", result.Contents[0].Role);
                Assert.Equal(text, result.Contents[0].Parts[0].Text);
                Assert.True("model" == result.Contents[1].Role || "assistant" == result.Contents[1].Role);

                HttpBase.Trace($"[SUMMARIZATION] model: {model}, Duration: {result.Duration:0.0}, ", this);

                // Conversation step 2
                var text2 = @"What is this conversation about?";

                var systemPrompt = @"You are a helpful assistant that analyzes English text";
                // ^^^ Change the system prompt to force the LLM to answer the question and do not improve the text.

                var result2 = client.Completions.TextImprovement(text: text2, language: "English", model: model, systemPrompt: systemPrompt, contents: result.Contents);
                Assert.Equal(4, result.Contents.Count); // Query + Response
                Assert.Equal("user", result.Contents[0].Role);
                Assert.True("model" == result.Contents[1].Role || "assistant" == result.Contents[1].Role);
                Assert.Equal("user", result.Contents[2].Role);
                Assert.True("model" == result.Contents[3].Role || "assistant" == result.Contents[1].Role);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void SummarizationResult_CountWords()
        {
            Assert.Equal(4, GenericAICompletions.SummarizationResult.CountWords("This is a test."));
            Assert.Equal(4, GenericAICompletions.SummarizationResult.CountWords("This was, a test."));
            Assert.Equal(4, GenericAICompletions.SummarizationResult.CountWords(@"This 
                                                                                  was 
                                                                                  a test."));
        }

        const string GlycemicReseachText = @"
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

        [Fact()]
        [TestBeforeAfter]
        public void Summarize_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            var expectedWords = DS.List("alice", "insurance", "car");

            foreach (var model in GenericAI.GetModels())
            {
                var client = new GenericAI();
                var result = client.Completions.Summarize(text: GlycemicReseachText, language: "English", model: model);
                HttpBase.Trace($"[SUMMARIZATION] model: {model}, Duration: {result.Duration:0.0}, %: {result.PercentageSummzarized}, TextWordCount: {result.TextWordCount}, SummaryWordCount: {result.SummaryWordCount}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateTitle_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            foreach (var model in GenericAI.GetModels())
            {
                var client = new GenericAI();
                var result = client.Completions.GenerateTitle(text: GlycemicReseachText, language: "English", model: model);
                HttpBase.Trace($"[GENERATE-TITLE] model: {model}, Duration: {result.Duration:0.0}, Text: {result.Title}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            foreach (var model in GenericAI.GetModels())
            {
                var client = new GenericAI();
                var result = client.Completions.Translate(text: GlycemicReseachText, language: "English", destinationLanguage:"French",  model: model);
                HttpBase.Trace($"[TRANSLATE] model: {model}, Duration: {result.Duration:0.0}, SourceText: {result.SourceText}, destLanguage: {result.TranslatedText}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateBulletPoints_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            foreach (var model in GenericAI.GetModels())
            {
                var client = new GenericAI();
                var result = client.Completions.GenerateBulletPoints(4, text: GlycemicReseachText, language: "English", model: model);
                HttpBase.Trace($"[GENERATE-BULLETPOINT] model: {model}, Duration: {result.Duration:0.0}, Text: {result.Text}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateBulletPoints_GenericAI_IsPassedTheWrongApiKey()
        {
            var model = "gpt-5-nano";
            var client = new GenericAI(ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY"));
            var result = client.Completions.GenerateBulletPoints(4, text: GlycemicReseachText, language: "English", model: model);
            Assert.Null(result.Text);
        }

        const string CSharpJsonDotNetQuestion = @"
When using C# and the newtonsoft library, what is the name of the attribute to serialize an enum as a string?
";
        [Fact()]
        [TestBeforeAfter]
        public void Conversation_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            foreach (var model in GenericAI.GetModels())
            {
                var client = new GenericAI();
                var result = client.Completions.Conversation(text: CSharpJsonDotNetQuestion, model: model);
                Assert.Contains("[JsonConverter(typeof(StringEnumConverter))]", result.Response);
                HttpBase.Trace($"[CONVERSATION] model: {model}, Duration: {result.Duration:0.0}, Response: {result.Response}", this);
            }
        }
    }
}