using DynamicSugar;
using fAI;
using fAI.OpenAIModel.ImageResponseGpt;
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

        const string TTS_TEXT = @"
diagnose the following patient as a spine orthopedic surgeon:
Jane Doe, a 55-year-old female, presents with extremely painful lower back pain and in the left legs.
MRI scan shows fracture at L4.
Find root cause.
";

        [Fact()]
        [TestBeforeAfter]
        public void GenericAISpeech_Create()
        {
            var client = new GenericAI();
            client.Speech.TTSVoiceInfos.Take(3).ToList().ForEach(request =>
            {
                foreach(var testVoice in request.TestsVoices)
                {
                    var inputFile = client.Speech.Create(TTS_TEXT, testVoice, request.Model, cost: request.ComputeCost(TTS_TEXT), useOpenAI: !request.OpenRouterSupported);
                    Assert.True(File.Exists(inputFile));
                }
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAISpeech_Create___fish_audio_s2_1_pro()
        {
            var TTS_TEXT = @"
diagnose the following patient as a spine orthopedic surgeon:
Jane Doe, a 55-year-old female, presents with extremely painful lower back pain and in the left legs.
MRI scan shows fracture at L4.
Find root cause.
";

            // https://fish.audio/app/default-voices/
            var client = new GenericAI();
            client.Speech.TTSVoiceInfos.Where(v => v.Model== "fish-audio/s2.1-pro").ToList().ForEach(request =>
            {
                foreach (var testVoice in request.TestsVoices)
                {
                    var inputFile = client.Speech.Create(TTS_TEXT, testVoice, request.Model, cost: request.ComputeCost(TTS_TEXT), useOpenAI: !request.OpenRouterSupported);
                    Assert.True(File.Exists(inputFile));
                }
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAISpeech_Create_WithCustomVoice()
        {
            var inputVoiceFileName = base.GetTestFile("Fred Voice Sample - With Compressor - I am Jordan Lee.wav");
            var inputVoiceText = @"Hey there everyone. I am Jordan Lee, and I'm super excited to be here with you today because I've got something to share with you that's going to blow your mind. Introducing the all new Swift Gadget X, the gadget of your dreams. This little marvel is not just a device, it's your personal assistant you're entertaining and HOB and your productivity powerhouse all rolled into one. Trust me folks, this isn't just your ordinary gadget. This is a game changer. Imagine having the world at your fingertips with lighting, fast performance, crystal clear display and a battery life that seems to go on forever. You won't miss a bit with Swift Gadget XD by your side. Now I know what you might be thinking, Jordan. This is too good to be true. But let me tell you. But let me tell you, we put Swift Gadget X through the ringer. We've tested it in extreme conditions, push it to limits, and it came out on top every single time. We believe in this product so much that we are offering an exclusive deal just for you, our online community. ";
            var client = new GenericAI();
            var modelSupportingCustomVoice = client.Speech.TTSVoiceInfos.Where(v => v.SupportVoiceCloning).ToList();
            modelSupportingCustomVoice.ForEach(request =>
            {
                try
                {
                    var inputFile = client.Speech.Create(TTS_TEXT, null, request.Model, cost: request.ComputeCost(TTS_TEXT), useOpenAI: !request.OpenRouterSupported,
                                                            inputVoiceFileName: inputVoiceFileName, inputVoiceText: inputVoiceText);
                    Assert.True(File.Exists(inputFile));
                }
                catch (Exception ex)
                {
                    HttpBase.Trace($"[ERROR] Exception: {ex.Message}", this);
                }
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAITranscription_Create_Mp3_en()
        {
            var client = new GenericAI();
            var mp3FileName = base.GetTestFile("TestFile.01.48Khz.mp3");
            var expected1 = "I am he as you are he as you are me. And we are all together. See how they run like pigs from a gun. See how they fly. I'm crying.";
            var expected2 = "I am he as you are he as you are me. And we are all together. See how they run like pigs from a gun. See how they fly. I Am crying.";

            client.Transcription.GetModels().ForEach(model =>
            {
                try
                {
                    var (text, usage) = client.Transcription.Create(mp3FileName, model: model);
                    Assert.True(
                        WhisperSpeechToTextEngineTests.ReplacePunctuation(expected1) == WhisperSpeechToTextEngineTests.ReplacePunctuation(text) ||
                        WhisperSpeechToTextEngineTests.ReplacePunctuation(expected2) == WhisperSpeechToTextEngineTests.ReplacePunctuation(text));
                }
                catch (Exception ex)
                {
                    HttpBase.Trace($"[ERROR] Model: {model}, Exception: {ex.Message}", this);
                }
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAITranscription_Create_wav_en()
        {
            var client = new GenericAI();
            var mp3FileName = base.GetTestFile("Fred Voice Sample - With Compressor - I am Jordan Lee.wav");
            var expected1 = "Hey there, everyone. I am Jordan Lee, and I'm super excited to be here with you today because I've got something to share with you that's going to blow your mind. Introducing the all-new Swift Gadget X, the gadget of your dreams. This little marvel is not just a device; it's your personal assistant, your entertainment hub, and your productivity powerhouse, all rolled into one. Trust me, folks, this isn't just your ordinary gadget. This is a game changer. Imagine having the world at your fingertips with lightning-fast performance, crystal-clear display, and a battery life that seems to go on forever. You won't miss a beat with Swift Gadget X by your side. Now, I know what you might be thinking, Jordan, this is too good to be true. But let me tell you, but let me tell you, we put Swift Gadget X through the ringer. We've tested it in extreme conditions, pushed it to the limits, and it came out on top every single time. We believe in this product so much that we are offering an exclusive deal just for you, our online community.";

            client.Transcription.GetModels().Take(1).ToList().ForEach(model =>
            {
                var (text, usage) = client.Transcription.Create(mp3FileName, model: model);
                Assert.True(WhisperSpeechToTextEngineTests.ReplacePunctuation(expected1) == WhisperSpeechToTextEngineTests.ReplacePunctuation(text));
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAITranscription_Create_wav_fr()
        {
            var client = new GenericAI();
            var mp3FileName = base.GetTestFile("Fred Voice Sample - With Compressor - EN FRANCAIS.ORIGINAL.wav");
            var expected1 = "L'automne débute fin septembre. Durant cette saison, les feuilles des arbres tombent et couvrent le sol d'un tapis brun, rouge et jaune. Le temps devient de plus en plus froid, il commence à y avoir de la pluie et du vent. C'est le moment de sortir son manteau et son parapluie. L'automne est aussi la saison des récoltes. On ramasse le maïs, le tournesol, les pommes et le raisin. Vient ensuite l'hiver, la saison la plus froide qui commence fin décembre. Le paysage devient tout blanc à cause de la neige et de la glace. Cette période marque l'arrivée de Noël et des fêtes de fin d'année. Avec le printemps qui commence en mars, le soleil est de retour et le temps se réchauffe. La nature redevient verte, l'herbe et les fleurs poussent à nouveau. Il est agréable de se promener pour observer les papillons, les abeilles et écouter le chant des oiseaux. Enfin, l'été arrive à la fin du mois de juin. C'est la saison la plus chaude pendant laquelle on recommence à mettre ses lunettes de soleil et à aller se promener sur la plage. C'est aussi le moment idéal pour profiter de l'eau et aller nager à la mer ou à la piscine.";

            client.Transcription.GetModels().Take(1).ToList().ForEach(model =>
            {
                var (text, usage) = client.Transcription.Create(mp3FileName, model: model, language: "fr");
                Assert.True(WhisperSpeechToTextEngineTests.ReplacePunctuation(expected1) == WhisperSpeechToTextEngineTests.ReplacePunctuation(text));
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAITranscription_Create_wav_en_SpeedStudy()
        {
            var client = new GenericAI();
            var mp3FileName = base.GetTestFile("Fred Voice Sample - With Compressor - I am Jordan Lee.wav");
            var expected1 = "Hey there, everyone. I am Jordan Lee, and I'm super excited to be here with you today because I've got something to share with you that's going to blow your mind. Introducing the all-new Swift Gadget X, the gadget of your dreams. This little marvel is not just a device; it's your personal assistant, your entertainment hub, and your productivity powerhouse, all rolled into one. Trust me, folks, this isn't just your ordinary gadget. This is a game changer. Imagine having the world at your fingertips with lightning-fast performance, crystal-clear display, and a battery life that seems to go on forever. You won't miss a beat with Swift Gadget X by your side. Now, I know what you might be thinking, Jordan, this is too good to be true. But let me tell you, but let me tell you, we put Swift Gadget X through the ringer. We've tested it in extreme conditions, pushed it to the limits, and it came out on top every single time. We believe in this product so much that we are offering an exclusive deal just for you, our online community.";

            client.Transcription.GetModels().ToList().ForEach(model =>
            {
                var (text, usage) = client.Transcription.Create(mp3FileName, model: model);
                DS.List("excited", "super", "jordan").ForEach(word => Assert.Contains(word, text.ToLowerInvariant()));
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAI_Credits()
        {
            var client = new GenericAI();
            var credits = client.Utility.GetCredits();
            Assert.NotNull(credits);
            Assert.True(credits.TotalCredits > 0);
            Assert.True(credits.CreditsRemaining > 0);
        }


        private static string ReplaceInvalidFileNameChars(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAI_Image__futuristic_city_skyline_at_sunset()
        {
            var imagePrompt = @"
A futuristic city skyline at sunset, 
with flying cars and neon lights, 
in the style of cyberpunk, highly detailed, 8k resolution
";
            var client = new GenericAI();
            client.Image.GetModelsApi().ForEach(model =>
            {
                try
                {
                    var imageFileName = Path.Combine(Path.GetTempPath(), $"{ReplaceInvalidFileNameChars(model)}.{Guid.NewGuid()}.jpg");
                    var (image, usage) = client.Image.Create(imagePrompt, model: model, filePath: imageFileName);
                    Assert.True(File.Exists(image));
                    Assert.True(usage.InputTokens > 0);
                    Assert.True(usage.OutputTokens > 0);
                }
                catch (Exception ex)
                {
                    HttpBase.Trace($"[ERROR] Model: {model}, Exception: {ex.Message}", this);
                }
            });
        }


        [Fact()]
        [TestBeforeAfter]
        public void GenericAI_Image__EricClaptonAndEs335()
        {
            var imagePrompt = @"
Create a realistic image based the following content.
IMPORTANT: Do not include any text, letters, numbers, logos, labels, signs, or watermarks anywhere in the image.
------------

MAKE THE IMAGE ONLY ABOUT ERIC CLAPTON.

# Gibson ES 335
## Famous Players and the Guitar's Cultural Rise
Famous Players and the Guitar's Cultural Rise
- Chuck Berry: Brought the 335 to mass attention ??? defined the vocabulary of rock and roll guitar.
- Eric Clapton: Used a 1964 ES-335 on the legendary Beano album with John Mayall's Bluesbreakers.
- Freddie King: Searing Texas blues tone ??? articulate but never sterile. A defining sound of the genre.
- Larry Carlton: Nicknamed 'Mr. 335' for his studio work in the 1970s spanning jazz, soul, and fusion.
- Alvin Lee: Blazed through 'I'm Going Home' at Woodstock 1969 ??? one of the festival's great moments. 
- B.B. King: His ES-355 sibling 'Lucille' cemented the semi-hollow as the blues guitar of choice.
------------
";
            var client = new GenericAI();
            client.Image.GetCheapModels().ForEach(model =>
            {
                try
                {
                    var imageFileName = Path.Combine(Path.GetTempPath(), $"{ReplaceInvalidFileNameChars(model)}.{Guid.NewGuid()}.jpg");
                    var (image, usage) = client.Image.Create(imagePrompt, model: model, filePath: imageFileName);
                    Assert.True(File.Exists(image));
                    Assert.True(usage.InputTokens > 0);
                    Assert.True(usage.OutputTokens > 0);
                }
                catch (Exception ex)
                {
                    HttpBase.Trace($"[ERROR] Model: {model}, Exception: {ex.Message}", this);
                }
            });
        }
    }
}