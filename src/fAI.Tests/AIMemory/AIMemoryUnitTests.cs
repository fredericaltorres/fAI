using DynamicSugar;
using fAI;
using fAI.AIMemory;
using Markdig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Xunit;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public partial class AIMemoryUnitTests : OpenAIUnitTestsBase
    {
        const string TestDBName = @"c:\temp\fai.aimemory.unittests.db";

        public AIMemoryUnitTests()
        {
            DeleteFile(TestDBName);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Add_Update_Search_Delete()
        {
            var aiMI = new AIMemoryManager(TestDBName);
            var aiMemory = new AIMemory()
            {
                PublishedUrl = "https://www.example.com/article1",
                Title = "Example Article 1",
                Text = "This is the text of the example article 1.",
                Type = PublishedDocumentInfoType.AI_Generated_Note,
                LocalFile = null,
            };

            aiMI.Add(Environment.GetEnvironmentVariable("OPENAI_API_KEY"), aiMemory);
            var aiMemory2 = aiMI.GetFromId(aiMemory.Id);
            Assert.NotNull(aiMemory2);
            Assert.Equal(aiMemory.PublishedUrl, aiMemory2.PublishedUrl);
            Assert.Equal(aiMemory.Title, aiMemory2.Title);
            Assert.Equal(aiMemory.Text, aiMemory2.Text);
            Assert.Equal(aiMemory.Type, aiMemory2.Type);
            Assert.Equal(aiMemory.LocalFile, aiMemory2.LocalFile);
        }
    }
}


