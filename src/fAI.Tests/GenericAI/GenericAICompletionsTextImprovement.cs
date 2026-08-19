using DynamicSugar;
using fAI;
using fAI.Util.Strings;
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
        //Regex _quickFilter = new Regex(AIMemoryManager.DEFAULT_MODEL_FOR_META_DATA_EXTRACTION);
        //Regex _quickFilter = new Regex("gemini-.*");
        Regex _quickFilter = new Regex("google/gemini-3.1-flash-lite");
        
        //Regex _quickFilter = null;

        public GenericAiCompletions_UnitTests()
        {
            OpenAI.TraceOn = true;
        }

        [Fact()]
        [TestBeforeAfter]
        public void ImproveEnglishText_GenericAI_InterfaceForOpenAIAndGoogle_ExperimentMultiMode()
        {
            try
            {
                var text = @"
hi Alice I wanted to let you know that I review the previous email about your car insurance policy I read the proposal I approved we can move on 
";
                var model = "gemini-3.1-flash-lite";
                var expectedWords = DS.List("alice", "insurance", "car");
                var client = new GenericAI();

                var result = client.Completions.TextImprovement(text: text, language: "English", model: model);

                Assert.True(expectedWords.All(w => result.Text.ToLower().Contains(w)));
                HttpBase.Trace($"[SUMMARIZATION] Model: {model}, Duration: {result.Duration:0.0}, ", this);

                Assert.True(client.Completions.LastUsage.InputTokens > 0);
                Assert.True(client.Completions.LastUsage.OutputTokens > 0);
            }
            catch (Exception ex)
            {
                HttpBase.Trace($"[ERROR]Exception: {ex.Message}", this);
            }
            finally
            {
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void ImproveEnglishText_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            var text = @"
hi Alice I wanted to let you know that I review the previous email about your car insurance policy I read the proposal I approved we can move on 
";
            var expectedWords = DS.List("alice", "insurance", "car");
            foreach (var model in GenericAI.GetModels(_quickFilter))
            {
                var client = new GenericAI();
                var result = client.Completions.TextImprovement(text: text, language: "English", model: model.Id);
                
                Assert.True(expectedWords.All(w => result.Text.ToLower().Contains(w)));
                HttpBase.Trace($"[SUMMARIZATION] Model: {model.Id}, Duration: {result.Duration:0.0}, ", this);

                Assert.True(client.Completions.LastUsage.InputTokens > 0);
                Assert.True(client.Completions.LastUsage.OutputTokens > 0);
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
            //var models = DS.List("gemini-2.0-flash", "claude-sonnet-4-5", "claude-haiku-4-5", "gpt-5-mini");

            foreach (var model in GenericAI.GetModels(_quickFilter))
            {
                var client = new GenericAI();
                // Conversation step 1
                var result = client.Completions.TextImprovement(text: text, language: "English", model: model.Id);
                Assert.True(expectedWords.All(w => result.Text.ToLower().Contains(w)));

                Assert.Equal(2, result.Contents.Count); // Query + Response
                Assert.Equal("user", result.Contents[0].Role);
                Assert.Equal(text, result.Contents[0].Parts[0].Text);
                Assert.True("model" == result.Contents[1].Role || "assistant" == result.Contents[1].Role);

                var systemPrompt = @"You are a helpful assistant that analyzes English text"; // <<< Change the system prompt to force the LLM to answer the question and do not improve the text.

                // Conversation step 2
                var text2 = @"What is this conversation about?";

                var result2 = client.Completions.TextImprovement(text: text2, language: "English", model: model.Id, systemPrompt: systemPrompt, contents: result.Contents);
                Assert.Equal(4, result2.Contents.Count); // Query + Response
                Assert.Equal("user", result2.Contents[0].Role);
                Assert.True("model" == result2.Contents[1].Role || "assistant" == result2.Contents[1].Role);
                Assert.Equal("user", result2.Contents[2].Role);
                Assert.True("model" == result2.Contents[3].Role || "assistant" == result2.Contents[1].Role);

                Assert.True(expectedWords.All(w => result2.Text.ToLower().Contains(w)));

                // Conversation step 3
                var text3 = @"is the car insurance proposal approved? Answer with YES or NO only.";

                var result3 = client.Completions.TextImprovement(text: text3, language: "English", model: model.Id, systemPrompt: systemPrompt, contents: result.Contents);
                Assert.Contains("yes", result3.Text.ToLower());

                Assert.Equal(6, result3.Contents.Count); // Query + Response
                Assert.Equal("user", result3.Contents[0].Role);
                Assert.True("model" == result3.Contents[1].Role || "assistant" == result3.Contents[1].Role);
                Assert.Equal("user", result3.Contents[2].Role);
                Assert.True("model" == result3.Contents[3].Role || "assistant" == result3.Contents[3].Role);
                Assert.Equal("user", result3.Contents[4].Role);
                Assert.True("model" == result3.Contents[5].Role || "assistant" == result3.Contents[5].Role);
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
            foreach (var model in GenericAI.GetModels(_quickFilter))
            {
                var client = new GenericAI();
                var result = client.Completions.Summarize(text: GlycemicReseachText, language: "English", model: model.Id, summarizeWordCount: 64);
                HttpBase.Trace($"[SUMMARIZATION] Duration: {result.Duration:00.00}, Model: {model.Id}, %: {result.PercentageSummzarized}, TextWordCount: {result.TextWordCount}, SummaryWordCount: {result.SummaryWordCount}", this);
                var cost = client.Completions.LastUsage.ComputeCost();
                Assert.True(cost > 0, $"Cost should be greater than 0 for model {model.Id}");
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void Summarize_GenericAI_OpenRouterModels()
        {
            var expectedWords = DS.List("alice", "insurance", "car");
            var models =  StringUtil.GetRandom(OpenRouter.GetModels().Select(m => m.Id).ToList(), 3);
            foreach (var model in models)
            {
                var client = new GenericAI();
                var result = client.Completions.Summarize(text: GlycemicReseachText, language: "English", model: model, summarizeWordCount: 64);
                HttpBase.Trace($"[SUMMARIZATION] Duration: {result.Duration:00.00}, Model: {model}, %: {result.PercentageSummzarized}, TextWordCount: {result.TextWordCount}, SummaryWordCount: {result.SummaryWordCount}, text: {result.Text}", this);

                var cost = client.Completions.LastUsage.ComputeCost();
                Assert.True(cost > 0, $"Cost should be greater than 0 for model {model}");
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateTitle_GenericAI_OpenRouterModels()
        {
            var models = StringUtil.GetRandom(OpenRouter.GetModels().Select(m => m.Id).ToList(), 3);

            foreach (var model in models)
            {
                var client = new GenericAI();
                var result = client.Completions.GenerateTitle(text: GlycemicReseachText, language: "English", model: model);
                HttpBase.Trace($"[GENERATE-TITLE] Duration: {result.Duration:00.00}, Model: {model}, Text: {result.Title}", this);
                var cost = client.Completions.LastUsage.ComputeCost();
                Assert.True(cost > 0, $"Cost should be greater than 0 for model {model}");
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateTitle_GenericAI_InterfaceFor_OpenAI_Google_Anthrophic_OpenRouterDeepSeek()
        {
            foreach (var model in GenericAI.GetModels(_quickFilter))
            {
                var client = new GenericAI();
                var result = client.Completions.GenerateTitle(text: GlycemicReseachText, language: "English", model: model.Id);
                HttpBase.Trace($"[GENERATE-TITLE] Duration: {result.Duration:0.00}, Model: {model.Id}, Text: {result.Title}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_GenericAI_OpenRouterModels()
        {
            var models = OpenRouter.GetModels().Take(4).ToList();

            foreach (var model in models)
            {
                var client = new GenericAI();
                var result = client.Completions.Translate(text: GlycemicReseachText, language: "English", destinationLanguage: "French", model: model.Id);
                HttpBase.Trace($"[TRANSLATE] Duration: {result.Duration:00.00}, Model: {model}, destLanguage: {result.TranslatedText}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateBulletPoints_GenericAI_OpenRouterModels()
        {
            var models = OpenRouter.GetModels().Take(4).ToList();

            foreach (var model in models)
            {
                var client = new GenericAI();
                var result = client.Completions.GenerateBulletPoints(4, text: GlycemicReseachText, language: "English", model: model.Id);
                Assert.NotNull(result.Text);
                HttpBase.Trace($"[GENERATE-BULLETPOINT] Duration: {result.Duration:00.00}, Model: {model}, Text: {result.Text}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            foreach (var model in GenericAI.GetModels(_quickFilter))
            {
                var client = new GenericAI();
                var result = client.Completions.Translate(text: GlycemicReseachText, language: "English", destinationLanguage: "French", model: model.Id);
                HttpBase.Trace($"[TRANSLATE] Model: {model}, Duration: {result.Duration:0.0}, SourceText: {result.SourceText}, destLanguage: {result.TranslatedText}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateBulletPoints_GenericAI_InterfaceForOpenAIAndGoogle()
        {
            foreach (var model in GenericAI.GetModels(_quickFilter))
            {
                var client = new GenericAI();
                var result = client.Completions.GenerateBulletPoints(4, text: GlycemicReseachText, language: "English", model: model.Id);
                HttpBase.Trace($"[GENERATE-BULLETPOINT] Model: {model}, Duration: {result.Duration:0.0}, Text: {result.Text}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateBulletPoints_GenericAI_IsPassedTheWrongApiKey()
        {
            var model = "gpt-5-nano";
            var client = new GenericAI(apiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY"));
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
            foreach (var model in GenericAI.GetModels(_quickFilter))
            {
                var client = new GenericAI();
                var result = client.Completions.Conversation(text: CSharpJsonDotNetQuestion, model: model.Id);
                Assert.Contains("[JsonConverter(typeof(StringEnumConverter))]", result.Response);
                HttpBase.Trace($"[CONVERSATION] Model: {model.Id}, Duration: {result.Duration:0.0}, Response: {result.Response}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void DetermineTheTypeOfPhrase()
        {
            GenericAI.GetModels(new Regex("gemini-3.1-flash-lite")).ForEach(model => // _quickFilter
            {
                AIPromptCache.Instance.Clear();
                var client = new GenericAI(); // ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY")

                Assert.Equal(GenericAICompletions.PhraseType.Order, client.Completions.DetermineTheTypeOfPhrase("Add a to-do item with the following title", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Paint the sky?", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("What is the color of the sky?", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Analyse as a Medical Doctor, Karin health issue and issue a diagnostic.", model: model.Id     ));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Recommend as a Medical Doctor, Karin health issue and issue a diagnostic.", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("What is my highest priority?", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("What is my highest priority?", model: model.Id));

                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("List the doctors whom diagnosticated Karen", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Research what Joe is working on today", model: model.Id));
                Assert.Equal(GenericAICompletions.PhraseType.Question, client.Completions.DetermineTheTypeOfPhrase("Tell me about Doctor StrangeLove", model: model.Id));
                
                Assert.Equal(GenericAICompletions.PhraseType.Statement, client.Completions.DetermineTheTypeOfPhrase("The sky is blue", model: model.Id));
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void RePhraseQuestionIntoAffirmation()
        {
            GenericAI.GetModels(_quickFilter).ForEach(model => //
            {
                try
                {
                    var client = new GenericAI(apiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY"));
                    var answer = client.Completions.RePhraseQuestionIntoAffirmation("What is my highest priority?", model: model.Id);
                    Assert.Contains("Your highest priority is __SOMETHING__", answer);
                    var j = answer.ToJSON();

                    answer = client.Completions.RePhraseQuestionIntoAffirmation("What is my next task to do?", model: model.Id);
                    Assert.Contains("Your next task to do is __SOMETHING__", answer);

                    answer = client.Completions.RePhraseQuestionIntoAffirmation("What is my next task to do with the highest priority?", model: model.Id);
                    Assert.Contains("Your next task to do with the highest priority is __SOMETHING__", answer);

                    answer = client.Completions.RePhraseQuestionIntoAffirmation("When is my next meeting?", model: model.Id);
                    Assert.True(
                        answer.Contains("Your next meeting is at __SOMETHING__") ||
                        answer.Contains("Your next meeting is __SOMETHING__")
                        );

                    answer = client.Completions.RePhraseQuestionIntoAffirmation("With whom is my next meeting?", model: model.Id);
                    Assert.Contains("Your next meeting is with __SOMETHING__", answer);
                }
                catch (Exception ex)
                {
                    HttpBase.Trace($"Model: {model}, Exception: {ex.Message}", this);
                }
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void FixPhrase()
        {
            GenericAI.GetModels(_quickFilter).ForEach(model =>
            {
                var client = new GenericAI(apiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY"));
                var fixedPhrase = client.Completions.FixPhrase("Your to-do number one in the personal section is  Taxes 2025", "English", model: model.Id);
                //Assert.Contains("Your next task to do is __SOMETHING__", fixedPhrase);
                fixedPhrase = client.Completions.FixPhrase("Your highest priority to-do in the personal section is  Create and sign a Will and Trust", "English", model: model.Id);
                fixedPhrase = client.Completions.FixPhrase("What you need to do about your car is  RAV4 Car oil change", "English", model: model.Id   );
            });
        }


        const string notes1 = @"
on January 15th, 2026, I had a meeting with John Smith about the new Salesforce integration project in Paris.
The meeting was at 10 AM and it lasted for 1 hour.
I need to prepare a presentation for the next meeting on July 20th, 2026
";

        [Fact()]
        [TestBeforeAfter]
        public void ExtractMetaData_1()
        {
            GenericAI.GetModels(_quickFilter).ForEach(model =>
            {
                var client = new GenericAI();
                var medataDictionary = client.Completions.ExtractMetaDataFromNotes(notes1, model: model.Id).MetaData;
                Assert.Equal("John Smith", medataDictionary["people"].First());
                Assert.Equal("Paris", medataDictionary["locations"].First());
                Assert.Equal("2026-01-15", medataDictionary["dates_mentioned"].First());
                Assert.Equal("Salesforce integration", medataDictionary["topics"].First());
                Assert.Equal("task", medataDictionary["type"].First());
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void ExtracatMetaData_1()
        {
            GenericAI.GetModels(_quickFilter).ForEach(model =>
            {
                var client = new GenericAI();
                var keywords = client.Completions.ExtractKeywordFromNotes(notes1, model: model.Id);
                Assert.True(keywords.Any());
            });
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

            GenericAI.GetModels(_quickFilter).ForEach(model =>
            {
                var client = new GenericAI(); // ApiKey: Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY")
                var metaData = client.Completions.ExtractMetaDataFromNotes(notes2, model: model.Id);
                ///////////Assert.True(metaData.Keywords.Any());

                var medataDictionary = metaData.MetaData;
                Assert.True(medataDictionary["people"].Any());
                Assert.True(medataDictionary["dates_mentioned"].Any());
                Assert.True(medataDictionary["action_items"].Any());
                Assert.True(medataDictionary["topics"].Any());
                Assert.Equal("task", medataDictionary["type"].First());
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void ConvertPdfToMarkdown()
        {
            var client = new GenericAI();
            var markDownText = client.Completions.ConvertPdfToMarkdown(@".\TestFiles\car policy.pdf");
            Assert.Contains("9415857", markDownText);
            Assert.Contains("ALICE TORRES", markDownText);
            Assert.Contains("153 HIGHLAND ST APT 3", markDownText);
        }

        [Fact()]
        [TestBeforeAfter]
        public void TextImprovement_GenericAI_WithSkill()
        {
            var text = @"
diagnose the following patient as a spine orthopedic surgeon:
Jane Doe, a 55-year-old female, presents with extremely painful lower back pain and in the left legs.
MRI scan shows fracture at L4.
Find root cause.
";

            DS.List("openai/gpt-5.6-terra", "gemini-3.1-flash-lite").ForEach(model =>
            {
                var client = new GenericAI();
                var result = client.Completions.TextImprovement(text: text, language: "English", model: model,
                                                                systemPrompt: "diagnose the following patient as a spine orthopedic surgeon:",
                                                                skillName: "spine-orthopedic-surgeon",
                                                                skillRootFolder: @"C:\DVT\fAI\src\fAI.Tests\TestFiles\Skills");

                HttpBase.Trace($"[SUMMARIZATION] Model: {model}, Duration: {result.Duration:0.0}, ", this);
                Assert.True(client.Completions.LastUsage.InputTokens > 0);
                Assert.True(client.Completions.LastUsage.OutputTokens > 0);
            });
        }


        public class TTSRequest
        {
            public string Input { get; set; }
            public List<string> Voices { get; set; } 
            public string TestVoice => Voices != null && Voices.Count > 0 ? Voices[0] : null;

            public List<string> TestsVoices
            {
                get
                {
                    if (Voices != null && Voices.Count > 0)
                        return new List<string>() { Voices[0], Voices[this.Voices.Count - 1] };
                    return new List<string>();
                }
            }
            public string Model { get; set; }
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAISpeech_Create()
        {
            var text = @"
diagnose the following patient as a spine orthopedic surgeon:
Jane Doe, a 55-year-old female, presents with extremely painful lower back pain and in the left legs.
MRI scan shows fracture at L4.
Find root cause.
";
            var ttsRequests = new List<TTSRequest>
            {
                new TTSRequest { Input = text, Model = "mistralai/voxtral-mini-tts-2603", Voices = new List<string>() {
                    "en_paul_neutral", "en_paul_sad" , "en_paul_happy" , "en_paul_frustrated" , "en_paul_excited" , "en_paul_confident" , "en_paul_cheerful" , "en_paul_angry",
                    "gb_oliver_neutral", "gb_oliver_sad" , "gb_oliver_excited" , "gb_oliver_curious" , "gb_oliver_confident" , "gb_oliver_cheerful" , "gb_oliver_angry", 
                    "fr_marie_sad" ,  "fr_marie_happy" , "fr_marie_excited" , "fr_marie_curious" , "fr_marie_angry","fr_marie_neutral",
                    "gb_jane_sarcasm" , "gb_jane_confused" , "gb_jane_shameful" , "gb_jane_sad" , "gb_jane_neutral" , "gb_jane_jealousy" , "gb_jane_frustrated" , "gb_jane_curious" , "gb_jane_confident"
                } },

                new TTSRequest { Input = text, Model = "x-ai/grok-voice-tts-1.0", Voices = new List<string>() { "eve", "ara", "rex", "sal", "leo" } },
                new TTSRequest { Input = text, Model = "deepgram/aura-2", Voices = new List<string>() { "aura-2-thalia-en", "aura-2-agathe-fr", "aura-2-agustina-es", "aura-2-alvaro-es", "aura-2-ama-ja", "aura-2-amalthea-en", "aura-2-andromeda-en", "aura-2-antonia-es", "aura-2-apollo-en", "aura-2-aquila-es", "aura-2-arcas-en", "aura-2-aries-en", "aura-2-asteria-en", "aura-2-athena-en", "aura-2-atlas-en", "aura-2-aurelia-de", "aura-2-aurora-en", "aura-2-beatrix-nl", "aura-2-callista-en", "aura-2-carina-es", "aura-2-celeste-es", "aura-2-cesare-it", "aura-2-cinzia-it", "aura-2-cora-en", "aura-2-cordelia-en", "aura-2-cornelia-nl", "aura-2-daphne-nl", "aura-2-delia-en", "aura-2-demetra-it", "aura-2-diana-es", "aura-2-dionisio-it", "aura-2-draco-en", "aura-2-ebisu-ja", "aura-2-elara-de", "aura-2-electra-en", "aura-2-elio-it", "aura-2-estrella-es", "aura-2-fabian-de", "aura-2-flavio-it", "aura-2-fujin-ja", "aura-2-gloria-es", "aura-2-harmonia-en", "aura-2-hector-fr", "aura-2-helena-en", "aura-2-hera-en", "aura-2-hermes-en", "aura-2-hestia-nl", "aura-2-hyperion-en", "aura-2-iris-en", "aura-2-izanami-ja", "aura-2-janus-en", "aura-2-javier-es", "aura-2-julius-de", "aura-2-juno-en", "aura-2-jupiter-en", "aura-2-kara-de", "aura-2-lara-de", "aura-2-lars-nl", "aura-2-leda-nl", "aura-2-livia-it", "aura-2-luciano-es", "aura-2-luna-en", "aura-2-maia-it", "aura-2-mars-en", "aura-2-melia-it", "aura-2-minerva-en", "aura-2-neptune-en", "aura-2-nestor-es", "aura-2-odysseus-en", "aura-2-olivia-es", "aura-2-ophelia-en", "aura-2-orion-en", "aura-2-orpheus-en", "aura-2-pandora-en", "aura-2-phoebe-en", "aura-2-pluto-en", "aura-2-rhea-nl", "aura-2-roman-nl", "aura-2-sander-nl", "aura-2-saturn-en", "aura-2-selena-es", "aura-2-selene-en", "aura-2-silvia-es", "aura-2-sirio-es", "aura-2-theia-en", "aura-2-uzume-ja", "aura-2-valerio-es", "aura-2-vesta-en", "aura-2-viktoria-de", "aura-2-zeus-en" } },
                new TTSRequest { Input = text, Model = "qwen/qwen-audio-3.0-tts-flash", Voices = new List<string>() { "longanhuan_v3.6", "loongjohn" } },
                new TTSRequest { Input = text, Model = "microsoft/mai-voice-2-flash", Voices = new List<string>() { "en-US-Harper:MAI-Voice-2", "es-MX-Valeria:MAI-Voice-2", "fr-FR-Soleil:MAI-Voice-2", "de-DE-Klaus:MAI-Voice-2" } },
                new TTSRequest { Input = text, Model = "deepgram/flux-tts:free", Voices = new List<string>() { "flux-alexis-en", "flux-bree-en", "flux-brittany-en", "flux-brooke-en", "flux-bruce-en", "flux-cliff-en", "flux-cole-en", "flux-colin-en", "flux-conor-en", "flux-donovan-en", "flux-drew-en", "flux-elise-en", "flux-gemma-en", "flux-haley-en", "flux-hannah-en", "flux-heather-en", "flux-jack-en", "flux-kai-en", "flux-kelsey-en", "flux-kit-en", "flux-maeve-en", "flux-marcelo-en", "flux-marcus-en", "flux-meena-en", "flux-meghan-en", "flux-miles-en", "flux-naveen-en", "flux-paige-en", "flux-priya-en", "flux-rufus-en", "flux-sean-en", "flux-sharon-en", "flux-sienna-en", "flux-tanner-en", "flux-wade-en", "flux-wes-en" } },
                new TTSRequest { Input = text, Model = "google/gemini-3.1-flash-tts-preview", Voices = new List<string>() {"Zephyr" , "Puck" , "Charon" , "Kore" , "Fenrir" , "Leda" , "Orus" , "Aoede" , "Callirrhoe" , "Autonoe" , "Enceladus" , "Iapetus" , "Umbriel" , "Algieba" , "Despina" , "Erinome" , "Algenib" , "Rasalgethi" , "Laomedeia" , "Achernar" , "Alnilam" , "Schedar" , "Gacrux" , "Pulcherrima" , "Achird" , "Zubenelgenubi" , "Vindemiatrix" , "Sadachbia" , "Sadaltager" , "Sulafat" } },
            };

            ttsRequests.ForEach(request =>
            {
                var client = new GenericAI();
                foreach(var testVoice in request.TestsVoices)
                {
                    var inputFile = client.GenericAISpeech.Create(request.Input, testVoice, request.Model);
                    Assert.True(File.Exists(inputFile));
                }
            });
        }
    }
}