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
    public class OpenAIAudioTranscriptionsTests
    {
        [Fact()]
        public void SpeechToText()
        {
            var mp3FileName = Path.Combine(".", "TestFiles", "TestFile.01.48Khz.mp3");
            var client = new OpenAI();
            var text = client.Audio.Transcriptions.Create(mp3FileName);
            var expected = "I am he as you are he as you are me. And we are all together. See how they run like pigs from a gun. See how they fly. I'm crying.";
            Assert.Equal(expected, text);
        }
    }
}