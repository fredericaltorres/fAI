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
    public class OpenAIAudioTextToSpeech
    {
        [Fact()]
        public void SpeechToText()
        {
            const string input = @"Maybe I'm amazed at the way you pulled me out of time. Hung me on a line.";
            var client = new OpenAI();
            var mp3FileName = client.Audio.Speech.Create(input, OpenAISpeech.Voices.echo);

            var mp3Info = AudioUtil.GetMp3Info(mp3FileName);
            Assert.True(mp3Info.DurationAsDouble > 3);

            AudioUtil.DeleteFile(mp3FileName);
        }
    }
}