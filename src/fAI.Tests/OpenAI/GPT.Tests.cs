using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.GPT;
using System.Runtime.InteropServices;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class GPTTests
    {
        [Fact()]
        public void GetModels()
        {
            var gpt = new GPT();
            var models = gpt.GetModels();
            Assert.True(models.data.Count > 0);
            Assert.True(models.data[0].Created < DateTime.Now);
        }

        const string error_log = @"[2023-11-07T23:53:14.260Z] 2023/11/07 06:53:10.995 PM | [ERROR]Verify .Folder.Description expected:#regex (@PresentationFile@)|(@AuthorUsername@) actual:IntegrationTesting_Converters_Cheetah_Company_06 objectType:Category - AssertDetails:regex:'(WordDocument_UniqueAzureStorageEncryption.docx)|(IntegrationTesting_Converters_Cheetah_Company_06_Admin)', value:'IntegrationTesting_Converters_Cheetah_Company_06'";
        const string info_success_log = @"2023-11-06 22:31:30.680|BrainRequin|Core|1.0.1.0|INFO|FTORRES-DL5560|msg=CEF:0|BrainRequin|Core|0|Message|Message|Info|msg=[SUCCEEDED 265493281, 10.0s] Deleting pid:533300675, type:HARD-DELETE, Method: Presentation.Delete(), ProcessId: 36336; ProcessName: BrainRequin.IntegrationTesting.Converters.Console; MachineName: FTORRES-DL5560; UserName: ftorres;";

        [Fact()]
        public void IsThis_AnLogError_No()
        {
            var r = new GPT().IsThis("As a software developer", "is this an error message", info_success_log);
            Assert.Equal(GPT_YesNoResponse.No, r);
        }

        [Fact()]
        public void IsThis_AnLogError_Yes()
        {
            var r = new GPT().IsThis("As a software developer", "is this an error message", error_log);
            Assert.Equal(GPT_YesNoResponse.Yes, r);
        }


        [Fact()]
        public void Completion_Chat_QuestionAboutPastSchedule()
        {
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
                // Url = "https://api.openai.com/v1/chat/completions"
            };
            var response = new GPT().ChatCompletionCreate(prompt);
            Assert.True(response.Success);
            Assert.True(response.Text.Contains(""));

            var blogPost = response.BlogPost;

            prompt.Messages.Add(new GPTMessage { Role = MessageRole.user, Content = "When was the last time I talked with Eric?" });
            response = new GPT().ChatCompletionCreate(prompt);
            Assert.True(Regex.IsMatch(response.Text, @"Eric.*09\/01\/2021 at 15:00"));

            prompt.Messages.Add(new GPTMessage { Role = MessageRole.user, Content = "What do I have to on 09/10/2021?" });
            response = new GPT().ChatCompletionCreate(prompt);
            Assert.True(Regex.IsMatch(response.Text, @"dog.*vet.*10:00"));
        }

        [Fact()]
        public void Completion_Chat_AnalyseLogError()
        {
            var prompt = new Prompt_GPT_4 {
                Messages = new List<GPTMessage> {
                    new GPTMessage { Role =  MessageRole.system, Content = "You are a helpful and experienced software developer."      },
                    new GPTMessage { Role =  MessageRole.user, Content = $"Analyse this error message:{Environment.NewLine}{error_log}" }
                },
                Url = "https://api.openai.com/v1/chat/completions"
            };
            var response = new GPT().ChatCompletionCreate(prompt);
            Assert.True(response.Success);
            Assert.True(response.Text.Contains("The error message indicates that there is a mismatch between the expected and actual values"));

            var blogPost = response.BlogPost;
            Assert.True(blogPost.Contains("Model:"));
            Assert.True(blogPost.Contains("Execution:"));

            var formattedBogPost = CompletionResponse.FormatChatGPTAnswerForTextDisplay(blogPost);
        }

        [Fact()]
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
zz zz zz zz";
            Assert.Equal(expectedBlogPost, formattedBogPost);
        }

        [Fact()]
        public void Complettion_Chat_Hello()
        {
            var p = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = "You are a helpful assistant." },
                    new GPTMessage{ Role =  MessageRole.user, Content = "Hello!" }
                },
                Url = "https://api.openai.com/v1/chat/completions"
            };
            var response = new GPT().ChatCompletionCreate(p);
            Assert.True(response.Success);
            Assert.Equal("Hello! How can I assist you today?", response.Text);

            p.Messages.Add(new GPTMessage { Role = MessageRole.user, Content = "What time is it?" });
            response = new GPT().ChatCompletionCreate(p);
            Assert.True(response.Text.Contains("real-time"));
        }

        [Fact()]
        public void Completion_ThisIsATest()
        {
            var p = new Prompt_GPT_35_Turbo 
            { 
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.user, Content = "Say this is a test!" }
                }
            };
            var response = new GPT().ChatCompletionCreate(p);
            Assert.True(response.Success);
            Assert.Equal("This is a test!", response.Text);
        }

        const string ReferenceEnglishSentence = "Hello world.";

        [Fact()]
        public void Translate_EnglishToFrench()
        {
            var gpt = new GPT();
            var translation = gpt.Translate(ReferenceEnglishSentence, TranslationLanguages.English, TranslationLanguages.French);
            Assert.Equal("Bonjour monde.", translation);
        }

        [Fact()]
        public void Translate_EnglishToSpanish()
        {
            var gpt = new GPT();
            var translation = gpt.Translate(ReferenceEnglishSentence, TranslationLanguages.English, TranslationLanguages.Spanish);
            Assert.Equal("'Hola mundo.'", translation);
        }

        const string ReferenceEnglishTextForSummarization = @"Hey there, everyone! I'm Jordan Lee, and I'm super excited to be here with you today because 
I've got somethin to share with you that is going to blow your mind!
 Introducing the all-new ""SwiftGadget X"" – the gadget of your dreams! This little marvel is not just a device; 
it's your personal assistant, your entertainment hub, and your productivity powerhouse, all rolled into one. 
Trust me, folks, this isn't your ordinary gadget – this is a game-changer. ";

        [Fact()]
        public void Summarize_EnglishText()
        {
            var gpt = new GPT();
            var summarization = gpt.Summarize(ReferenceEnglishTextForSummarization, TranslationLanguages.English);
            var expected = "Jordan Lee is excited to introduce the \"SwiftGadget X\", a versatile and innovative device that serves as a personal assistant, entertainment hub, and productivity tool. It is not an ordinary gadget, but a game-changer.";
            Assert.NotEqual(null, summarization);
        }

        [Fact()]
        public void Prompt()
        {
            //var gpt = new GPT();
            //const string ReferenceEnglishTextForSummarization = @"Q: What's the diameter of the earth? A:";
            //var answer = gpt.Prompt(ReferenceEnglishTextForSummarization);
        }
    }
}