using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.OpenAICompletions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using DynamicSugar;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class ChainTests
    {
        [Fact()]
        [TestBeforeAfter]
        public void Chain_TellThreeInterrestingFactAbout()
        {
            var chain = new Chain();
            var prompt = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage> 
                {
                    new GPTMessage { Role =  MessageRole.system, Content = "You are a helpful assistant." },
                    new GPTMessage { Role =  MessageRole.user, Content = "Tell Three Interresting Fact About [Subject]" },
                }
            };
            var text = chain.Prompt(prompt)
                            .Invoke(new { Subject = "Elvis" })
                            .Text;
            Assert.Contains("Elvis", text);
        }
    }
}