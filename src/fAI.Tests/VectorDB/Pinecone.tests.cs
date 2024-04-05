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
using Newtonsoft.Json;
using DynamicSugar;
using static System.Net.Mime.MediaTypeNames;
using static fAI.OpenAICompletionsEx;
using fAI.VectorDB;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class PineconeTests : OpenAiCompletionsBase
    {
        public PineconeTests()
        {
            OpenAI.TraceOn = true;
        }
         
        [Fact()]
        [TestBeforeAfter]
        public void AnswerQuestionBasedOnText_Answered()
        {
            var client = new PineconeDB();
            var indexName = "unit-test-index"; 
            client.CreateIndex(indexName);
        }
    }
}



