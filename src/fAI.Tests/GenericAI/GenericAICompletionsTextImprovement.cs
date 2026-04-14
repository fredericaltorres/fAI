using DynamicSugar;
using fAI;
using Markdig;
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
                HttpBase.Trace($"[SUMMARIZATION] Model: {model}, Duration: {result.Duration:0.0}, ", this);
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
            var models = DS.List("gemini-2.0-flash", "claude-sonnet-4-5", "claude-haiku-4-5", "gpt-5-mini");

            foreach (var model in models)
            {
                var sw = Stopwatch.StartNew();
                var client = new GenericAI();

                // Conversation step 1
                var result = client.Completions.TextImprovement(text: text, language: "English", model: model);
                Assert.True(expectedWords.All(w => result.Text.ToLower().Contains(w)));

                Assert.Equal(2, result.Contents.Count); // Query + Response
                Assert.Equal("user", result.Contents[0].Role);
                Assert.Equal(text, result.Contents[0].Parts[0].Text);
                Assert.True("model" == result.Contents[1].Role || "assistant" == result.Contents[1].Role);

                var systemPrompt = @"You are a helpful assistant that analyzes English text"; // <<< Change the system prompt to force the LLM to answer the question and do not improve the text.

                // Conversation step 2
                var text2 = @"What is this conversation about?";

                var result2 = client.Completions.TextImprovement(text: text2, language: "English", model: model, systemPrompt: systemPrompt, contents: result.Contents);
                Assert.Equal(4, result2.Contents.Count); // Query + Response
                Assert.Equal("user", result2.Contents[0].Role);
                Assert.True("model" == result2.Contents[1].Role || "assistant" == result2.Contents[1].Role);
                Assert.Equal("user", result2.Contents[2].Role);
                Assert.True("model" == result2.Contents[3].Role || "assistant" == result2.Contents[1].Role);

                Assert.True(expectedWords.All(w => result2.Text.ToLower().Contains(w)));

                // Conversation step 3
                var text3 = @"is the car insurance proposal approved? Answer with YES or NO only.";

                var result3 = client.Completions.TextImprovement(text: text3, language: "English", model: model, systemPrompt: systemPrompt, contents: result.Contents);
                Assert.Contains("yes", result3.Text.ToLower());

                Assert.Equal(6, result3.Contents.Count); // Query + Response
                Assert.Equal("user", result3.Contents[0].Role);
                Assert.True("model" == result3.Contents[1].Role || "assistant" == result3.Contents[1].Role);
                Assert.Equal("user", result3.Contents[2].Role);
                Assert.True("model" == result3.Contents[3].Role || "assistant" == result3.Contents[3].Role);
                Assert.Equal("user", result3.Contents[4].Role);
                Assert.True("model" == result3.Contents[5].Role || "assistant" == result3.Contents[5].Role);

                sw.Stop();
                HttpBase.Trace($"[CONVERSATION] Model: {model}, Duration: {sw.ElapsedMilliseconds / 1000:0.0}, ", this);
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
                HttpBase.Trace($"[SUMMARIZATION] Model: {model}, Duration: {result.Duration:0.0}, %: {result.PercentageSummzarized}, TextWordCount: {result.TextWordCount}, SummaryWordCount: {result.SummaryWordCount}", this);
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
                HttpBase.Trace($"[GENERATE-TITLE] Model: {model}, Duration: {result.Duration:0.0}, Text: {result.Title}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            foreach (var model in GenericAI.GetModels())
            {
                var client = new GenericAI();
                var result = client.Completions.Translate(text: GlycemicReseachText, language: "English", destinationLanguage: "French", model: model);
                HttpBase.Trace($"[TRANSLATE] Model: {model}, Duration: {result.Duration:0.0}, SourceText: {result.SourceText}, destLanguage: {result.TranslatedText}", this);
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
                HttpBase.Trace($"[GENERATE-BULLETPOINT] Model: {model}, Duration: {result.Duration:0.0}, Text: {result.Text}", this);
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
                HttpBase.Trace($"[CONVERSATION] Model: {model}, Duration: {result.Duration:0.0}, Response: {result.Response}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void DetermineTheTypeOfPhrase()
        {
            var model = "gemini-2.0-flash";
            var client = new GenericAI(ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY"));
            Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("What is my highest priority?", model: model));

            Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("List the doctors whom diagnosticated Karen", model: model));
            Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Research what Joe is working on today", model: model));

            Assert.Equal(GenericAICompletions.PhraseType.Order, client.Completions.DetermineTheTypeOfPhrase("Add a to-do item with the following title", model: model));
            Assert.Equal(GenericAICompletions.PhraseType.Statement, client.Completions.DetermineTheTypeOfPhrase("The sky is blue", model: model));
        }

        [Fact()]
        [TestBeforeAfter]
        public void RePhraseQuestionIntoAffirmation()
        {
            var model = "gemini-2.0-flash";
            var client = new GenericAI(ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY"));
            var answer = client.Completions.RePhraseQuestionIntoAffirmation("What is my highest priority?", model: model);
            Assert.Contains("Your highest priority is __SOMETHING__", answer);
            var j = answer.ToJSON();

            answer = client.Completions.RePhraseQuestionIntoAffirmation("What is my next task to do?", model: model);
            Assert.Contains("Your next task to do is __SOMETHING__", answer);

            answer = client.Completions.RePhraseQuestionIntoAffirmation("What is my next task to do with the highest priority?", model: model);
            Assert.Contains("Your next task to do with the highest priority is __SOMETHING__", answer);

            answer = client.Completions.RePhraseQuestionIntoAffirmation("When is my next meeting?", model: model);
            Assert.Contains("Your next meeting is at __SOMETHING__", answer);

            answer = client.Completions.RePhraseQuestionIntoAffirmation("With whom is my next meeting?", model: model);
            Assert.Contains("Your next meeting is with __SOMETHING__", answer);
        }

        [Fact()]
        [TestBeforeAfter]
        public void FixPhrase()
        {
            var model = "gemini-2.0-flash";
            var client = new GenericAI(ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY"));
            var fixedPhrase = client.Completions.FixPhrase("Your to-do number one in the personal section is  Taxes 2025", "English", model: model);
            //Assert.Contains("Your next task to do is __SOMETHING__", fixedPhrase);
            fixedPhrase = client.Completions.FixPhrase("Your highest priority to-do in the personal section is  Create and sign a Will and Trust", "English", model: model);
            fixedPhrase = client.Completions.FixPhrase("What you need to do about your car is  RAV4 Car oil change", "English", model: model);
        }

        [Fact()]
        [TestBeforeAfter]
        public void ExtractMetaData_1()
        {
            var model = "gemini-2.0-flash";
            var client = new GenericAI(ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY"));

            var notes1 = @"
on January 15th, 2026, I had a meeting with John Smith about the new Salesforce integration project in Paris.
The meeting was at 10 AM and it lasted for 1 hour.
I need to prepare a presentation for the next meeting on July 20th, 2026
";
            var medataDictionary = client.Completions.ExtractMetaDataFromNotes(notes1, model: model).MetaData;
            Assert.Equal("John Smith", medataDictionary["people"].First());
            Assert.Equal("Paris", medataDictionary["locations"].First());
            Assert.Equal("2026-01-15", medataDictionary["dates_mentioned"].First());
            Assert.Equal("Salesforce integration", medataDictionary["topics"].First());
            Assert.Equal("task", medataDictionary["type"].First());
        }


        [Fact()]
        [TestBeforeAfter]
        public void ExtractMetaData_2()
        {
            var notes2 = @"
On March 3rd, 2026, I had an extended strategy session with 
Sarah Mitchell, 
David Chen, and the newly onboarded project lead, Rebecca Torres, 

regarding the long-overdue overhaul of our legacy CRM platform and 
its proposed integration with both the Salesforce Enterprise suite and 
the third-party analytics tool, DataBridge Pro. 

The meeting, originally scheduled for 9:00 AM in Conference Room B, 
was pushed back by forty-five minutes due to a last-minute conflict with 
David's call with the Singapore office, 

ultimately running well past its allotted two-hour window and wrapping up closer to 12:30 PM. 

During the session, we reviewed the preliminary scoping document that 
Rebecca had circulated the previous 
Thursday, 
flagged several unresolved dependencies around the legacy data migration, 
and agreed that the engineering team would need at least three additional weeks to complete 
their technical audit before any development work could begin. 

Following up on action items, 
I need to revise the project timeline and 
budget estimates in collaboration with the finance liaison, 
Mark Huang, and submit a consolidated report to 
the VP of Operations no later than April 11th, 2026. 

Additionally, 
Sarah has requested that I prepare a detailed risk assessment and a stakeholder presentation, 
both of which are due before our next cross-functional review meeting, 
currently penciled in for May 7th, 2026 at 2:00 PM, 
with a follow-up executive briefing tentatively set for the week of June 22nd, 2026.";

            var models = DS.List("gemini-2.0-flash", "gpt-5.2", "claude-opus-4-6");

            foreach (var model in models)
            {
                var client = new GenericAI(); // ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY")
                var medataDictionary = client.Completions.ExtractMetaDataFromNotes(notes2, model: model).MetaData;
                Assert.Equal(4, medataDictionary["people"].Count);
                Assert.Equal(4, medataDictionary["dates_mentioned"].Count);
                Assert.Equal(2, medataDictionary["action_items"].Count);
                Assert.Equal(3, medataDictionary["topics"].Count);
                Assert.Equal("task", medataDictionary["type"].First());
            }
        }




        [Fact()]
        [TestBeforeAfter]
        public void ExtractMetaData_claude_opus_4_6_FAST_AndNotFast()
        {
            var notes2 = @"
On March 3rd, 2026, I had an extended strategy session with 
Sarah Mitchell, 
David Chen, and the newly onboarded project lead, Rebecca Torres, 

regarding the long-overdue overhaul of our legacy CRM platform and 
its proposed integration with both the Salesforce Enterprise suite and 
the third-party analytics tool, DataBridge Pro. 

The meeting, originally scheduled for 9:00 AM in Conference Room B, 
was pushed back by forty-five minutes due to a last-minute conflict with 
David's call with the Singapore office, 

ultimately running well past its allotted two-hour window and wrapping up closer to 12:30 PM. 

During the session, we reviewed the preliminary scoping document that 
Rebecca had circulated the previous 
Thursday, 
flagged several unresolved dependencies around the legacy data migration, 
and agreed that the engineering team would need at least three additional weeks to complete 
their technical audit before any development work could begin. 

Following up on action items, 
I need to revise the project timeline and 
budget estimates in collaboration with the finance liaison, 
Mark Huang, and submit a consolidated report to 
the VP of Operations no later than April 11th, 2026. 

Additionally, 
Sarah has requested that I prepare a detailed risk assessment and a stakeholder presentation, 
both of which are due before our next cross-functional review meeting, 
currently penciled in for May 7th, 2026 at 2:00 PM, 
with a follow-up executive briefing tentatively set for the week of June 22nd, 2026.";

            var models = DS.List("claude-opus-4-6-fast", "claude-opus-4-6");

            foreach (var model in models)
            {
                var client = new GenericAI(); // ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY")

                var medataDictionary = client.Completions.ExtractMetaDataFromNotes(notes2, model: model).MetaData;
                Assert.True(medataDictionary["people"].Count > 1);
                Assert.True(medataDictionary["dates_mentioned"].Count > 1);
                Assert.True(medataDictionary["action_items"].Count > 1);
                Assert.True(medataDictionary["topics"].Count > 1);
                Assert.Equal("task", medataDictionary["type"].First());
            }
        }
    }
}