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
    public class WhisperSpeechToTextEngineTests : UnitTestBase
    {
        [Fact()]
        public void SpeechToText_Mp3_File()
        {
            var mp3FileName = base.GetTestFile("TestFile.01.48Khz.mp3");
            var s = new WhisperSpeechToText();
            var result = s.ExtractText(mp3FileName, "en", false);
            var expected = "I am he as you are he as you are me. And we are all together. See how they run like pigs from a gun. See how they fly. I'm crying.";
            Assert.True(result.Success);
            Assert.Equal(expected, result.Text);
            Assert.Equal("en", result.Language);
        }

        [Fact()]
        public void SpeechToText_Mp3_File_VTT()
        {
            var mp3FileName = base.GetTestFile("TestFile.01.48Khz.mp3");
            var s = new WhisperSpeechToText();
            var result = s.ExtractText(mp3FileName, "en", true);
            var expected = @"WEBVTT

00:00:00.000 --> 00:00:04.240
I am he as you are he as you are me. And we are all together.

00:00:05.200 --> 00:00:11.680
See how they run like pigs from a gun. See how they fly. I'm crying.
";
            Assert.True(result.Success);
            Assert.Equal(expected.Replace(Environment.NewLine,"\n").Trim(), result.Captions.Trim());
            Assert.Equal("en", result.Language);
            Assert.True(result.Captions.Length > 100);
            Assert.Contains("-->", result.Captions);
            Assert.Contains("WEBVTT", result.Captions);
        }

        [Fact()]
        public void SpeechToText_Mp3_Url()
        {
            var mp3Url = "https://fredcloud.blob.core.windows.net/public/Fred.Video/fAI/AudioTest01.mp3";
            var s = new WhisperSpeechToText();
            var result = s.ExtractText(mp3Url, "en", false);
            var expected = "You're playing cards, Trump said. You're gambling with the lives of millions of people. You're gambling with World War III. You're gambling with World War III. And what you're doing is very disrespectful to the country, this country.";
            Assert.True(result.Success);
            Assert.Equal(expected, result.Text);
            Assert.Equal("en", result.Language);
        }

        [Fact()]
        public void SpeechToText_Mp4_File()
        {
            var mp4FileName = base.GetTestFile("I am Frederic Torres 2025.mp4");
            var s = new WhisperSpeechToText();
            var result = s.ExtractText(mp4FileName, "en", false);
            var expected = "I am Frederick Torres. I am a software engineer. I never wrote a book about software. I never taught at a university. I am just a software engineer.";
            Assert.True(result.Success);
            Assert.Equal(expected, result.Text);
            Assert.Equal("en", result.Language);
        }

    }
}