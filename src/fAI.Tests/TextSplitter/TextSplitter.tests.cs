using System.Collections.Generic;
using System;
using Xunit;
using DynamicSugar;
using System.Linq;
using System.IO;

namespace fAI.Tests
{
    public class TextSplitterTestFixture : OpenAIUnitTestsBase, IDisposable 
    {
        public TextSplitterTestFixture()
        {
        }

        public void Dispose()
        {
        }
    }

    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class TextSplitter : IClassFixture<PineconeTestFixture>
    {
        public TextSplitter()
        {
            OpenAI.TraceOn = true;
        }

        [Fact()]
        [TestBeforeAfter]
        public void SplitIntoParagraphs()
        {
            var inputFileName = File.ReadAllText(@"C:\DVT\fAI\src\fAI.Tests\SpeechToText\LongText.txt");
            var paragraphs = fAI.Util.TextSplitter.SplitIntoParagraphs(inputFileName, 128, 4096, 32);
            ///paragraphs.ToFile(@"C:\DVT\fAI\src\fAI.Tests\SpeechToText\LongText.splitted.txt");
            Assert.Equal(55, paragraphs.Count);
            var expectedParagraphs = new List<string>().FromFile(@"C:\DVT\fAI\src\fAI.Tests\SpeechToText\LongText.splitted.txt");

            for (int i = 0; i < paragraphs.Count; i++)
                Assert.Equal(expectedParagraphs[i], paragraphs[i]);
        }
    }
}
