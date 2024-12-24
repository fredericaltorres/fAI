using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.OpenAICompletions;
using System.Runtime.InteropServices;
using DynamicSugar;

namespace fAI.Tests
{

    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class DeepgramAudioTranscriptionsTests : UnitTestBase
    {
      
        [Fact()]
        public void SpeechToText()
        {
            var mp3FileName = base.GetTestFile("TestFile.01.48Khz.mp3");
            var client = new DeepgramAI();
            var r = client.Audio.Transcriptions.Create(mp3FileName);
            var expected = "I am he as you are he as you are me, and we are all together. See how they run like pigs from a gun. See how they fly. I'm crying.";
            Assert.Equal(FlexStrCompare(expected), FlexStrCompare(r.Text));
        }

        [Fact()]
        public void SpeechToText_WithVTT()
        {
            var mp3FileName = base.GetTestFile("TestFile.01.48Khz.mp3");
            var client = new DeepgramAI();
            var r = client.Audio.Transcriptions.Create(mp3FileName, vtt: true);
            var expected = "I am he as you are he as you are me, and we are all together. See how they run like pigs from a gun. See how they fly. I'm crying.";
            Assert.Equal(FlexStrCompare(expected), FlexStrCompare(r.Text));
            Assert.StartsWith("WEBVTT", r.VTT);
            Assert.Contains("00:00:00 --> 00:00:02.260", r.VTT);
            Assert.Contains(FlexStrCompare(" - I am he as you are he as you are me,"), FlexStrCompare(r.VTT));
        }

        public static bool PlayAudio(string filePath)
        {
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(filePath);
                return true;
            }
            else return false;
        }

        [Fact()]
        public void TextToSpeech()
        {
            using(var tfh = new TestFileHelper()) 
            {
                var client = new DeepgramAI();
                var text = "I am he as you are he as you are me, and we are all together. See how they run like pigs from a gun. See how they fly. I'm crying.";
                var mp3 = tfh.GetTempFileName(".mp3");
                client.Audio.TextToSpeech.CreateAsync(text, mp3).GetAwaiter().GetResult();
                PlayAudio(mp3);
            }
        }
    }
}