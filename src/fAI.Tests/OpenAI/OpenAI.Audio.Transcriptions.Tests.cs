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
        }
    }
}