using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.OpenAICompletions;
using System.Runtime.InteropServices;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class SpeechToTextEngineTests
    {
        [Fact()]
        public void SpeechToText()
        {
            var mp3FileName = Path.Combine(".", "TestFiles", "TestFile.01.48Khz.mp3");
            var s = new SpeechToTextEngine();
            var result = s.ExtractTextFromFile(mp3FileName, "en");
            var expected = "I am he as you are. He. As you are me, and we are all together. See how they run like pigs from a gun. See how they fly. I'm crying.";
            Assert.Equal(expected, result.Text);
        }
    }
}