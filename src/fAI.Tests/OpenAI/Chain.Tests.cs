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
    class FactDB  : IChainable
    {
        // List 5 facts about James bond and 5 fact about Elvis Presley, in a C# sharp dictionary use unique integer as key starting at 1000

        // List 5 facts about James bond and 5 fact about Elvis Presley in a JSON object use unique integer as key starting at 1000
        Dictionary<int, string> _facts = new Dictionary<int, string>();

        public void SetTextData()
        {
            _facts = new Dictionary<int, string>
            {
                // James Bond Facts
                { 1000, "(James Bond Fact) Creation by Ian Fleming: James Bond, code number 007, was created by British author Ian Fleming in 1952."},
                { 1001, "(James Bond Fact) First Appearance in 'Casino Royale': Bond debuted in Fleming's novel 'Casino Royale,' published in 1953."},
                { 1002, "(James Bond Fact) Iconic Code Number, 007: Bond's '00' prefix indicates a license to kill in the line of duty."},
                { 1003, "(James Bond Fact) Film Adaptations: The Bond film series began with 'Dr. No' in 1962."},
                { 1004, "J(James Bond Fact) Signature Drink: 'Vesper Martini,' described in 'Casino Royale' as a mix of Gordon's, vodka, and Kina Lillet."},

                // Elvis Presley Facts
                { 1005, "(Elvis Presley Fact) King of Rock and Roll: Elvis Presley is known as the 'King of Rock and Roll' and is one of the most significant cultural icons of the 20th century."},
                { 1006, "(Elvis Presley Fact) First Hit Single: His first hit single, 'Heartbreak Hotel,' was released in 1956 and became a number-one hit in the U.S."},
                { 1007, "(Elvis Presley Fact) Graceland: Elvis's home, Graceland, in Memphis, Tennessee, is one of the most visited private homes in America."},
                { 1008, "(Elvis Presley Fact) Film Career: Presley starred in 33 successful films, further establishing his fame and influence."},
                { 1009, "(Elvis Presley Fact) Best-Selling Solo Artist: Elvis is recognized by Guinness World Records as the best-selling solo music artist of all time."}
            };
        }

        public Dictionary<int, string> Facts => _facts;

        public FactDB AddFacts(string text, bool randomizeOrder = false, bool clear = false)
        {
            if(clear)
                _facts.Clear();
            var lines = text.SplitByCRLF().Where(s => !string.IsNullOrEmpty(s.Trim())).ToList();
            if(randomizeOrder)
                lines = lines.OrderBy(_ => Guid.NewGuid()).ToList();

            foreach (var line in lines)
                _facts.Add(_facts.Count + 1000, line);

            return this;
        }

        public string GetText(string regExg = null, string title = null)
        {
            var r = Retreive(regExg);
            var s = string.Join(Environment.NewLine, r);
            if(title != null)
                s = $"{title}{Environment.NewLine}{s}";
            return s;
        }
        public List<string> Retreive(string regExg)
        {
            var r = new List<string>();
            if (regExg == null)
            {
                r.AddRange(_facts.Values);
            }
            else
            {
                var regEx = new Regex(regExg, RegexOptions.IgnoreCase);
                foreach (var f in _facts)
                    if (regEx.IsMatch(f.Value))
                        r.Add(f.Value);
            }
            return r;
        }

        public void Randomize()
        {
            var l = Retreive(null);
            var s = string.Join(Environment.NewLine, l);
            AddFacts(s, true, true);
        }
        
        public string Invoke(string query)
        {
            return GetText(query);
        }

        public FactDB()
        {
        }

    }

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
            var text = chain.Invoke(prompt, new { Subject = "Elvis" }) .Text;
            Assert.Contains("Elvis", text);
        }


        [Fact()]
        [TestBeforeAfter]
        public void Chain_JamesBond()
        {
            var factDB = new FactDB();
            factDB.SetTextData();
            IChainable chainableFactDB  = factDB;
            var chain = new Chain();
            var prompt = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage>
                {
                    new GPTMessage { Role =  MessageRole.system, Content = "You are a British intelligence expert." },
                    new GPTMessage { Role =  MessageRole.user,   Content = @"
Answer the question based only on the following context:
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