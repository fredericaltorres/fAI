using DynamicSugar;
using fAI;
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
            var aiManager = new AIMemoryManager(TestDBName);
            var aiMemory = new AIMemory()
            {
                PublishedUrl = "https://www.example.com/article1",
                Title = "Example Article 1",
                Text = "This is the text of the example article 1.",
                Type = PublishedDocumentInfoType.AI_Generated_Note,
                LocalFile = null,
            };

            aiManager.Add(aiMemory, Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

            var __id__ = aiMemory.Id;

            VerifyAIMemoryInDB(aiManager, aiMemory);

            aiMemory.PublishedUrl += " - Updated";
            aiMemory.Title += " - Updated";
            aiMemory.Text += " - Updated";

            

            aiManager.Update(aiMemory);

            VerifyAIMemoryInDB(aiManager, aiMemory);

            aiManager.Delete(aiMemory);
            aiMemory = aiManager.GetFromId(aiMemory.Id);
            Assert.Null(aiMemory);
        }

        private static void VerifyAIMemoryInDB(AIMemoryManager aiMI, AIMemory aiMemory)
        {
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


