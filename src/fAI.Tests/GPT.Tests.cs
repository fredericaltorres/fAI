using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;

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



        [Fact()]
        public void Completion_Chat_AnalyseLogError()
        {
            const string error_log = @"[2023-11-07T23:53:14.260Z] 2023/11/07 06:53:10.995 PM | [ERROR]Verify .Folder.Description expected:#regex (@PresentationFile@)|(@AuthorUsername@) actual:IntegrationTesting_Converters_Cheetah_Company_06 objectType:Category - AssertDetails:regex:'(WordDocument_UniqueAzureStorageEncryption.docx)|(IntegrationTesting_Converters_Cheetah_Company_06_Admin)', value:'IntegrationTesting_Converters_Cheetah_Company_06'";
            var p = new Prompt_GPT_4 //Prompt_GPT_35_Turbo
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = "You are a helpful and experienced software developer." },
                    new GPTMessage{ Role =  MessageRole.user, Content = $"Analyse this error message:{Environment.NewLine}{error_log}" }
                },
                Url = "https://api.openai.com/v1/chat/completions"
            };
            var gpt = new GPT();
            var response = gpt.ChatCompletionCreate(p);
            Assert.True(response.Success);
            Assert.Equal("The error message indicates that there is a mismatch between the expected and actual values", response.Text);

            
        }

        [Fact()]
        public void Complettion_Chat_Hello()
        {
            var p = new Prompt_GPT_4 //Prompt_GPT_35_Turbo
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = "You are a helpful assistant." },
                    new GPTMessage{ Role =  MessageRole.user, Content = "Hello!" }
                },
                Url = "https://api.openai.com/v1/chat/completions"
            };
            var gpt = new GPT();
            var response = gpt.ChatCompletionCreate(p);
            Assert.True(response.Success);
            Assert.Equal("Hello! How can I assist you today?", response.Text);

            p.Messages.Add(new GPTMessage { Role = MessageRole.user, Content = "What time is it?" });
            response = gpt.ChatCompletionCreate(p);
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
            var gpt = new GPT();  
            var response = gpt.ChatCompletionCreate(p);
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
        public void Translate_EnglishToSpaninsh()
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