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
            var expected = "Jordan Lee is introducing the \"SwiftGadget X\", a multi-functional gadget that is not just a device but also a personal assistant, entertainment hub, and productivity powerhouse. It is described as a game-changer.";
            Assert.Equal(expected, summarization);
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