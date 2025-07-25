﻿using System.Collections.Generic;
using System.Linq;
using System;
using Xunit;
using static fAI.OpenAICompletions;
using Newtonsoft.Json;
using DynamicSugar;
using static fAI.OpenAICompletionsEx;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class OpenAiCompletionsChatUnitTests : OpenAIUnitTestsBase
    {
        public OpenAiCompletionsChatUnitTests()
        {
            OpenAI.TraceOn = true;
        }

        [Fact()]
        [TestBeforeAfter]
        public void GetModels()
        {
            var client = new OpenAI();
            var models = client.Completions.GetModels();
            Assert.True(models.data.Count > 0);
            Assert.True(models.data[0].Created < DateTime.Now);
        }

        const string error_log = @"2023/11/07 06:53:10.995 PM|[ERROR]Verify .Order.Description expected:#regex (Camembert|Toillete) actual:Gorgonzola, objectType:PurchaseOrder - AssertDetails:regex:'(Camembert|Toillete)', value:'Gorgonzola'";
        const string info_success_log = @"2023-11-06 22:31:30.680|Requin|INFO|FTORRES-12345|Info|Messgae=[SUCCEEDED 10.0s] Deleting order:53330, Method: DeleteOrder(), ProcessId: 36336; ProcessName: OrderProcessingConsole.exe; MachineName: FTORRES-12345; UserName: ftorres;";

        [Fact()]
        [TestBeforeAfter]
        public void IsThis_AnLogError_No()
        {
            var client = new OpenAI();
            var r = client.Completions.IsThis("As a software developer", "is this an error message", info_success_log);
            Assert.Equal(GPT_YesNoResponse.No, r);
        }

        [Fact()]
        [TestBeforeAfter]
        public void IsThis_AnLogError_Yes()
        {
            var client = new OpenAI();
            var r = client.Completions.IsThis("As a software developer", "is this an error message", error_log);
            Assert.Equal(GPT_YesNoResponse.Yes, r);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_Chat_QuestionAboutPastSchedule()
        {
            var client = new OpenAI();
            var prompt = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage>
                {
                    new GPTMessage { Role =  MessageRole.system, Content = "You are a helpful assistant." },
                    new GPTMessage { Role =  MessageRole.user, Content = $"08/02/2021 15:00 Meeting with Eric." },
                    new GPTMessage { Role =  MessageRole.user, Content = $"09/01/2021 15:00 Meeting with Eric." },
                    new GPTMessage { Role =  MessageRole.user, Content = $"09/10/2021 10:00 Take the dog to the vet." },
                    new GPTMessage { Role =  MessageRole.user, Content = $"09/20/2021 15:00 Meeting with Rick and John" },
                }
            };
            var response = client.Completions.Create(prompt);
            Assert.True(response.Success);
            Assert.Contains("", response.Text);

            var blogPost = response.BlogPost;

            prompt.Messages.Add(new GPTMessage { Role = MessageRole.user, Content = "When was the last time I talked with Eric?" });
            response = client.Completions.Create(prompt);
            Assert.Matches(@"Eric.*09\/01\/2021 at 15:00", response.Text);

            prompt.Messages.Add(new GPTMessage { Role = MessageRole.user, Content = "What do I have to do on 09/10/2021?" });
            response = client.Completions.Create(prompt);
            Assert.Matches(@"dog.*vet.*10:00", response.Text);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_Chat_AnalyseLogError()
        {
            var client = new OpenAI();
            var prompt = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage> {
                    new GPTMessage { Role =  MessageRole.system, Content = "You are a helpful and experienced software developer."      },
                    new GPTMessage { Role =  MessageRole.user, Content = $"Analyse this error message:{Environment.NewLine}{error_log}" }
                }
            };
            var response = client.Completions.Create(prompt);
            Assert.True(response.Success);
            DS.Assert.Words(response.Text, "error & message & mismatch & Order & expecting & Description & Camembert & Gorgonzola & Toillete");

            var blogPost = response.BlogPost;
            Assert.Contains("Model:", blogPost);
            Assert.Contains("Execution:", blogPost);

            var answer = response.Answer;
            Assert.Contains("Answer:", blogPost);

            var formattedBogPost = CompletionResponse.FormatChatGPTAnswerForTextDisplay(blogPost);
        }

        [Fact()]
        [TestBeforeAfter]
        public void FormatChatGPTAnswerForTextDisplay_MultiPhraseOnSameLine()
        {
            var blogPost = @"
Answer:
aa aa aa. 
aa aa aa. bb bb bb.
aa aa aa. bb bb bb. cc cc cc.
zz zz zz zz
";
            var formattedBogPost = CompletionResponse.FormatChatGPTAnswerForTextDisplay(blogPost);

            var expectedBlogPost = @"Answer:
aa aa aa.
aa aa aa.
bb bb bb.
aa aa aa.
bb bb bb.
cc cc cc.
zz zz zz zz
";
            Assert.Equal(expectedBlogPost, formattedBogPost);
        }


        [Fact()]
        public void FormatChatGPTAnswerForTextDisplay_BulletPoint()
        {
            var blogPost = @"
Answer:
aa aa aa.
aa aa aa. bb bb bb.
1. point A. Point A-1 continuation. Point A-2 continuation
2. point B. Point B-1 continuation. Point B-2 continuation
End of text
";
            var formattedBogPost = CompletionResponse.FormatChatGPTAnswerForTextDisplay(blogPost);

            var expectedBlogPost = @"Answer:
aa aa aa.
aa aa aa.
bb bb bb.
1. point A.
   Point A-1 continuation.
   Point A-2 continuation
2. point B.
   Point B-1 continuation.
   Point B-2 continuation
End of text
";
            Assert.Equal(expectedBlogPost, formattedBogPost);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_Chat_Hello()
        {
            var client = new OpenAI();
            var p = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = "You are a helpful assistant." },
                    new GPTMessage{ Role =  MessageRole.user, Content = "Hello!" }
                }
            };
            var response = client.Completions.Create(p);
            Assert.True(response.Success);
            Assert.Equal("Hello! How can I assist you today?", response.Text);

            p.Messages.Add(new GPTMessage { Role = MessageRole.user, Content = "What time is it?" });
            response = client.Completions.Create(p);
            Assert.Contains("real-time", response.Text);
            DS.Assert.Words(response.Text, "real-time & capabilities");
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_Chat_Hello_o()
        {
            var client = new OpenAI();
            var p = new Prompt_GPT_4o
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = "You are a JavaScript expert." },
                    new GPTMessage{ Role =  MessageRole.user, Content = "Write a JavaScript function usable in Node.js that take as argument a filename and returns the text content of the file." }
                }
            };
            var response = client.Completions.Create(p);
            Assert.True(response.Success);
            Assert.Contains("require('fs')", response.Text);
            Assert.Contains("fs.readFile(filename", response.Text);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_JsonMode_WorldCup()
        {
            var client = new OpenAI();
            var p = new Prompt_GPT_35_Turbo_JsonAnswer
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage { Role =  MessageRole.system, Content = "You are a helpful assistant designed to output JSON." },
                    new GPTMessage { Role =  MessageRole.user,   Content = "Who won the soccer world cup in 1998?" }
                }
            };
            var response = client.Completions.Create(p);
            Assert.True(response.Success);
            var answer = response.JsonObject["winner"];
            Assert.Equal("France", answer);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_JsonMode_WorldSeries()
        {
            var client = new OpenAI();
            var p = new Prompt_GPT_35_Turbo_JsonAnswer
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = "You are a helpful assistant designed to output JSON." },
                    new GPTMessage{ Role =  MessageRole.user, Content = "Who won the world series in 2020?" }
                }
            };
            var response = client.Completions.Create(p);
            Assert.True(response.Success);
            var answer = response.JsonObject["winner"];
            Assert.Equal("Los Angeles Dodgers", answer);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_ThisIsATest()
        {
            var client = new OpenAI();
            var p = new Prompt_GPT_35_Turbo
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.user, Content = "Say this is a test!" }
                }
            };
            var response = client.Completions.Create(p);
            Assert.True(response.Success);
            Assert.Equal("This is a test!", response.Text);
        }


        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_SpecialRules()
        {
            var client = new OpenAI();
            var text = "10%";
            Assert.False(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");

            text = "10% cheaper";
            Assert.True(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");

            text = "200K";
            Assert.False(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");

            text = "200K cheaper";
            Assert.True(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");

            text = "$200K";
            Assert.True(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");

            text = "3,948";
            Assert.True(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");

            text = "3,948,123";
            Assert.True(client.Completions.NeedToBeTranslated(text, TranslationLanguages.English, TranslationLanguages.French), $"NeedToBeTranslated text:{text}");
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_SpecialCase_NumberWithCommaAsThousandSeparator()
        {
            var client = new OpenAI();
            var text = "3,948";
            var translation = client.Completions.Translate(text, TranslationLanguages.English, TranslationLanguages.French);
            Assert.True(FlexStrCompare("3 948") == FlexStrCompare(translation));
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_SpecialCase_Dollars200K()
        {
            var client = new OpenAI();
            var text = "$200K";
            var translation = client.Completions.Translate(text, TranslationLanguages.English, TranslationLanguages.French);
            Assert.Equal("200 000 $", translation);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench()
        {
            var client = new OpenAI();
            var translation = client.Completions.Translate(ReferenceEnglishSentence, TranslationLanguages.English, TranslationLanguages.French);
            Assert.True(FlexStrCompare("Bonjour monde.") == FlexStrCompare(translation) ||
                        FlexStrCompare("Bonjour le monde.") == FlexStrCompare(translation));
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_Dictionary()
        {
            var client = new OpenAI();
            var inputDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReferenceEnglishJsonDictionary);
            var outputDictionary = client.Completions.Translate(inputDictionary, TranslationLanguages.English, TranslationLanguages.French);
            Assert.Equal(6, outputDictionary.Keys.Count);

            inputDictionary.Keys.ToList().ForEach(k => Assert.True(outputDictionary.ContainsKey(k)));

            Assert.Equal("Éducation", outputDictionary["2"]);
            Assert.Equal("Salle de classe 01", outputDictionary["3"]);
        }


        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_List()
        {
            var client = new OpenAI();
            var inputDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReferenceEnglishJsonDictionary);
            var inputList = inputDictionary.Values.ToList();

            var outputList = client.Completions.Translate(inputList, TranslationLanguages.English, TranslationLanguages.French);
            Assert.Equal(6, outputList.Count);

            AssertWords(outputList[0], "personnes,important,domaine,activité");
            //AssertWords(outputList[4], "Graphiques,entreprise");
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToFrench_ListOfNumber()
        {
            var client = new OpenAI();
            var inputList = DS.List(1, 2, 3, 4).Select(i => i.ToString()).ToList();

            var outputList = client.Completions.Translate(inputList, TranslationLanguages.English, TranslationLanguages.French);
            Assert.Equal(4, outputList.Count);
            DS.Range(4).ForEach(i => Assert.Contains((i + 1).ToString(), outputList[i]));
        }

        [Fact()]
        [TestBeforeAfter]
        public void Translate_EnglishToSpanish()
        {
            var client = new OpenAI();
            var translation = client.Completions.Translate(ReferenceEnglishSentence, TranslationLanguages.English, TranslationLanguages.Spanish);
            Assert.Equal("'Hola mundo.'", translation);
        }

        const string ReferenceEnglishTextForSummarization = @"Hey there, everyone! I'm Jordan Lee, and I'm super excited to be here with you today because 
I've got somethin to share with you that is going to blow your mind!
 Introducing the all-new ""SwiftGadget X"" – the gadget of your dreams! This little marvel is not just a device; 
it's your personal assistant, your entertainment hub, and your productivity powerhouse, all rolled into one. 
Trust me, folks, this isn't your ordinary gadget – this is a game-changer. ";

        [Fact()]
        [TestBeforeAfter]
        public void Summarize_EnglishText()
        {
            var client = new OpenAI();
            var summarization = client.CompletionsEx.Summarize(ReferenceEnglishTextForSummarization, TranslationLanguages.English);
            DS.Assert.Words(summarization, "introduc & SwiftGadget & (revolutionary | revolutionize) ");
            Assert.NotNull(summarization);
        }

        //[Fact()]
        //[TestBeforeAfter]
        //public void Summarize_EnglishText_SematicFunction()
        //{
        //    var client = new OpenAI();
        //    var summarizeFunc = client.CompletionsEx.SemanticFunction("Summarize the following text:\r\n[text]");
        //    var summarization = summarizeFunc(new { text = ReferenceEnglishTextForSummarization });
        //    Assert.NotNull(summarization);
        //}

        [Fact()]
        [TestBeforeAfter]
        public void Summarize_EnglishText_InOneLine()
        {
            var client = new OpenAI();
            var summarization = client.CompletionsEx.Summarize(ReferenceEnglishTextForSummarization, TranslationLanguages.English,
                                    promptCommand: "Summarize the following text in one line:");
            Assert.NotNull(summarization);
            DS.Assert.Words(summarization, "introduc & SwiftGadget & (revolutionary | revolutionize | versatile | game-changing | game-changer) ");
        }

        [Fact()]
        [TestBeforeAfter]
        public void Summarize_CopyRightedText_EnglishText()
        {
            string text = @"
Another suburban family morning
Grandmother screaming at the wall
We have to shout above the din of our rice crispies
We can't hear anything at all";

            var client = new OpenAI();
            var summarization = client.CompletionsEx.Summarize(text, TranslationLanguages.English);
            var result1 = "A chaotic morning in a suburban family where the grandmother is yelling and the noise of breakfast cereal makes it hard to hear anything.";
            var result2 = "The text describes a chaotic morning in a suburban family, with the grandmother yelling and the noise of their breakfast cereal making it difficult to hear anything.";

            DS.Assert.Words(summarization, "chaotic & morning & noise & cereal & grandmother");
        }

        const string KingOfFrances = @"
            ""Hugh Capet"" was king of France from 987 to 996.
            ""Robert II"" was king of France from 996 to 1031.
            ""Henry I"" was king of France from 1031 to 1060.
            ""Philip I"" was king of France from 1060 to 1108.
            ""Louis VI"" was king of France from 1108 to 1137.
            ""Louis VII"" was king of France from 1137 to 1180.
            ""Philip II"" was king of France from 1180 to 1223.
            ""Louis VIII"" was king of France from 1223 to 1226.
            ""Louis IX (Saint Louis)"" was king of France from 1226 to 1270.
            ""Philip III"" was king of France from 1270 to 1285.
            ""Philip IV"" was king of France from 1285 to 1314.
            ""Louis X"" was king of France from 1314 to 1316.
            ""John I"" was king of France from 1316 to 1316.
            ""Philip V"" was king of France from 1316 to 1322.
            ""Charles IV"" was king of France from 1322 to 1328.
            ""Philip VI (Valois Branch)"" was king of France from 1328 to 1350.
            ""John II"" was king of France from 1350 to 1364.
            ""Charles V"" was king of France from 1364 to 1380.
            ""Charles VI"" was king of France from 1380 to 1422.
            ""Charles VII"" was king of France from 1422 to 1461.
            ""Louis XI"" was king of France from 1461 to 1483.
            ""Charles VIII"" was king of France from 1483 to 1498.
            ""Louis XII"" was king of France from 1498 to 1515.
            ""Francis I"" was king of France from 1515 to 1547.
            ""Henry II"" was king of France from 1547 to 1559.
            ""Francis II"" was king of France from 1559 to 1560.
            ""Charles IX"" was king of France from 1560 to 1574.
            ""Henry III"" was king of France from 1574 to 1589.
            ""Henry IV (Bourbon Branch)"" was king of France from 1589 to 1610.
            ""Louis XIII"" was king of France from 1610 to 1643.
            ""Louis XIV"" was king of France from 1643 to 1715.
            ""Louis XV"" was king of France from 1715 to 1774.
            ""Louis XVI"" was king of France from 1774 to 1792.";

        [Fact()]
        [TestBeforeAfter]
        public void AnswerQuestionBasedOnText_Answered()
        {
            var client = new OpenAI();
            var question = "Who was king of france in 1032?";
            var answer = client.CompletionsEx.AnswerQuestionBasedOnText(KingOfFrances, question);
            Assert.Equal("Henry I", answer);
        }

        [Fact(Skip = "Do not pass with gpt 3.5")]
        [TestBeforeAfter]
        public void AnswerQuestionBasedOnText_Answered_GPT35()
        {
            var client = new OpenAI();
            var question = "Who was king of france in 1032?";
            var answer = client.CompletionsEx.AnswerQuestionBasedOnText(KingOfFrances, question, gpt35: true);
            // Assert.Equal("Henry I", answer);
        }

        [Fact()]
        [TestBeforeAfter]
        public void AnswerQuestionBasedOnText_AnswerNotFound()
        {
            var client = new OpenAI();
            var question = "Who was king of france in 2016?";
            var answer = client.CompletionsEx.AnswerQuestionBasedOnText(KingOfFrances, question);
            Assert.Equal(client.CompletionsEx.AnswerNotFoundDefault, answer);
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateOneMultiChoiceQuestionAboutText()
        {
            var questionCount = 1;
            var dbFact = new DBFact();
            dbFact.AddFacts(KingOfFrances, randomizeOrder: true);

            var client = new OpenAI();
            var questions = client.CompletionsEx.GenerateMultiChoiceQuestionAboutText(questionCount, dbFact.GetText());
            Assert.Equal(questionCount, questions.Count);

            var question = questions[0];
            Assert.True(question.Text.Length > 0);
            Assert.True(question.Answers.Count > 0);
            Assert.True(question.CorrectAnswerIndex >= 0 && question.CorrectAnswerIndex < question.Answers.Count);
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateThreeMultiChoiceQuestionAboutText()
        {
            var questionCount = 3;
            var dbFact = new DBFact().AddFacts(KingOfFrances, randomizeOrder: true);
            var client = new OpenAI();
            var questions = client.CompletionsEx.GenerateMultiChoiceQuestionAboutText(questionCount, dbFact.GetText());
            Assert.Equal(questionCount, questions.Count);

            foreach (var question in questions)
            {
                Assert.True(question.Text.Length > 0);
                Assert.True(question.Answers.Count > 0);
                Assert.True(question.CorrectAnswerIndex >= 0 && question.CorrectAnswerIndex < question.Answers.Count);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void MultiChoiceQuestion_FromText_OneAnswer()
        {
            var text = "Who was the king of France from 1285 to 1314?\n\nA) Louis IX\nB) Philip III\nC) Philip IV*\nD) Louis X";
            var q = MultiChoiceQuestion.FromText(text, 1)[0];
            Assert.Equal("Who was the king of France from 1285 to 1314?", q.Text);
            Assert.Equal("Louis IX", q.Answers[0]);
            Assert.Equal("Philip III", q.Answers[1]);
            Assert.Equal("Philip IV", q.Answers[2]);
            Assert.Equal("Louis X", q.Answers[3]);
            Assert.Equal(2, q.CorrectAnswerIndex);
        }

        [Fact()]
        [TestBeforeAfter]
        public void MultiChoiceQuestion_FromText_ThreeAnswer()
        {
            var text = @"
1. Who was the king of France from 1285 to 1314?
A. Louis XIII
B. Philip IV*
C. Charles V
D. John II

2. Which king ruled France from 1422 to 1461?
A. Louis IX (Saint Louis)
B. Charles VII*
C. Henry IV (Bourbon Branch)
D. Philip VI (Valois Branch)

3. When did Louis XIV reign as king of France?
A. 1643 to 1715*
B. 1515 to 1547
C. 1285 to 1314
D. 1574 to 1589
";
            var questions = MultiChoiceQuestion.FromText(text, 3);
            Assert.Equal(3, questions.Count);
            var q = questions[0];
            Assert.Equal("Who was the king of France from 1285 to 1314?", q.Text);
            Assert.Equal("Louis XIII", q.Answers[0]);
            Assert.Equal("Philip IV", q.Answers[1]);
            Assert.Equal("Charles V", q.Answers[2]);
            Assert.Equal("John II", q.Answers[3]);
            Assert.Equal(1, q.CorrectAnswerIndex);

            q = questions[2];
            Assert.Equal("When did Louis XIV reign as king of France?", q.Text);
            Assert.Equal("1643 to 1715", q.Answers[0]);
            Assert.Equal("1515 to 1547", q.Answers[1]);
            Assert.Equal("1285 to 1314", q.Answers[2]);
            Assert.Equal("1574 to 1589", q.Answers[3]);
            Assert.Equal(0, q.CorrectAnswerIndex);
        }

        /*
        [Fact()]
        [TestBeforeAfter]
        public void GenerateMultiChoiceQuestionAboutText_Chain_DBFact()
        {
            var dbFact = new FactDB();
            dbFact.AddFacts(KingOfFrances);

            IChainable factDB = dbFact;
            var chain = new Chain();
            var client = new OpenAI();
            var prompt = client.Completions.GetPromptForGenerateMultiChoiceQuestionAboutText(string.Join(Environment.NewLine, dbFact.Facts.Values));

            var answer = chain
                            .Invoke(factDB, new { Randomize = true })
                            .Invoke(factDB, new { Query = chain.NULL }) // Get all facts
                            .Invoke(prompt, new {  }) // Context = chain.InvokedText, Question = ""
                            .Text;

            var question = MultiChoiceQuestion.FromText(answer);

            Assert.True(question.Text.Length > 0);
            Assert.True(question.Answers.Count > 0);
            Assert.True(question.CorrectAnswerIndex >= 0 && question.CorrectAnswerIndex < question.Answers.Count);

        }
*/
    }
}




//const string KingOfFrances = @"
//""Hugh Capet"" was king of France from 987 to 996.
//""Robert II"" was king of France from 996 to 1031.
//""Henry I"" was king of France from 1031 to 1060.
//""Philip I"" was king of France from 1060 to 1108.
//""Louis VI"" was king of France from 1108 to 1137.
//""Louis VII"" was king of France from 1137 to 1180.
//""Philip II"" was king of France from 1180 to 1223.
//""Louis VIII"" was king of France from 1223 to 1226.
//""Louis IX (Saint Louis)"" was king of France from 1226 to 1270.
//""Philip III"" was king of France from 1270 to 1285.
//""Philip IV"" was king of France from 1285 to 1314.
//""Louis X"" was king of France from 1314 to 1316.
//""John I"" was king of France from 1316 to 1316.
//""Philip V"" was king of France from 1316 to 1322.
//""Charles IV"" was king of France from 1322 to 1328.
//""Philip VI (Valois Branch)"" was king of France from 1328 to 1350.
//""John II"" was king of France from 1350 to 1364.
//""Charles V"" was king of France from 1364 to 1380.
//""Charles VI"" was king of France from 1380 to 1422.
//""Charles VII"" was king of France from 1422 to 1461.
//""Louis XI"" was king of France from 1461 to 1483.
//""Charles VIII"" was king of France from 1483 to 1498.
//""Louis XII"" was king of France from 1498 to 1515.
//""Francis I"" was king of France from 1515 to 1547.
//""Henry II"" was king of France from 1547 to 1559.
//""Francis II"" was king of France from 1559 to 1560.
//""Charles IX"" was king of France from 1560 to 1574.
//""Henry III"" was king of France from 1574 to 1589.
//""Henry IV (Bourbon Branch)"" was king of France from 1589 to 1610.
//""Louis XIII"" was king of France from 1610 to 1643.
//""Louis XIV"" was king of France from 1643 to 1715.
//""Louis XV"" was king of France from 1715 to 1774.
//""Louis XVI"" was king of France from 1774 to 1792.
//";