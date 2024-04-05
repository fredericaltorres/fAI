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

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class OpenAIEmbeddingsTests
    {
        [Fact()]
        public void Embeddings_Create()
        {
            var input = "I am he as you are he as you are me. And we are all together. See how they run like pigs from a gun. See how they fly. I'm crying.";
            var client = new OpenAI();
            var r = client.Embeddings.Create(input);
            Assert.Equal("list", r.Object);
            Assert.Single(r.Data);
            Assert.Equal("embedding", r.Data[0].Object);
            Assert.Equal(0, r.Data[0].Index);
            Assert.Equal(r.Data[0].EmbeddingMaxValue, r.Data[0].Embedding.Count);
            Assert.Equal(37, r.Usage.PromptTokens);
            Assert.Equal(37, r.Usage.TotalTokens);
        }

        [Fact()]
        public void Embeddings_CreateTextVectors()
        {
            var client = new OpenAI();

            var input = "Hello world.";
            var r = client.Embeddings.Create(input);
            Debug.WriteLine(r.GenerateCSharpCode("HelloWorld"));
            Debug.WriteLine("");
        }
    }
}