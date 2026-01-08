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
            var text = chain.Invoke(prompt, new { Subject = "Elvis Presley" }) .Text;
            Assert.Contains("king", text.ToLower());
            Assert.Contains("rock", text.ToLower());
            Assert.Contains("roll", text.ToLower());
        }

        [Fact()]
        [TestBeforeAfter]
        public void Chain_JamesBond()
        {
            var factDB = new DBFact();
            factDB.SetTextData();
            IChainable chainableFactDB  = factDB;
            var chain = new Chain();
            var prompt = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage>
                {
                    new GPTMessage { Role =  MessageRole.system, Content = "You are a British intelligence expert." },
                    new GPTMessage { Role =  MessageRole.user,   Content = @"
Text the question based only on the following context:
[Context]

Question: [Question]
" },
                }
            };

            var personName = "James Bond";
            var question = $"Who is {personName}?";
            var text = chain
                            .Invoke(chainableFactDB, new { Query =  "(James Bond)|(Bond)" })
                            .Invoke(prompt, new { Context = chain.InvokedText, Question = question })
                            .Text;
            Assert.Contains("Bond", text);
            Assert.Contains("character", text);
            Assert.Contains("Ian Fleming", text);
        }
    }
}