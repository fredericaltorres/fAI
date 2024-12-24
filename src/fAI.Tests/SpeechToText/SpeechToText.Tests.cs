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
    public class SpeechToTextEngineTests : UnitTestBase
    {
        [Fact()]
        public void SpeechToText_Mp3_File()
        {
            var mp3FileName = base.GetTestFile("TestFile.01.48Khz.mp3");
            var s = new SpeechToTextEngine();
            var result = s.ExtractText(mp3FileName, "en", true);
            var expected = "I am he as you are. He. As you are me, and we are all together. See how they run like pigs from a gun. See how they fly. I'm crying.";
            Assert.True(result.Success);
            Assert.Equal(expected, result.Text);
            Assert.True(result.Captions.Length > 100);
            Assert.Contains("-->", result.Captions);
            Assert.Contains("WEBVTT", result.Captions);
        }

        [Fact()]
        public void SpeechToText_Mp4_File()
        {
            var mp4FileName = base.GetTestFile("I am Frederic Torres.mp4");
            var s = new SpeechToTextEngine();
            var result = s.ExtractText(mp4FileName, "en", true);
            var expected = "I am Fredrik Torres. I am a software engineer. I never wrote a book about software. I never taught at a university. I am just a software engineer. I.";
            Assert.True(result.Success);
            Assert.Equal(expected, result.Text);
            Assert.True(result.Captions.Length > 100);
            Assert.Contains("-->", result.Captions);
            Assert.Contains("WEBVTT", result.Captions);
        }

        [Fact()]
        public void SpeechToText_Mp4_Url()
        {
            var mp4Url = "https://fredcloud.blob.core.windows.net/public/Fred.Video/I%20am%20Frederic%20Torres.mp4";
            var s = new SpeechToTextEngine();
            var result = s.ExtractText(mp4Url, "en", true);
            var expected = "I am Fredrik Torres. I am a software engineer. I never wrote a book about software. I never taught at a university. I am just a software engineer. I.";
            Assert.True(result.Success);
            Assert.Equal(expected, result.Text);
            Assert.True(result.Captions.Length > 100);
            Assert.Contains("-->", result.Captions);
            Assert.Contains("WEBVTT", result.Captions);
        }
    }
}