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

            OpenAISpeech.VoicesAsString.ToList().ForEach(voiceName =>
            {
                var mp3FileName = client.Audio.Speech.Create(input, voiceName);
                var mp3Info = AudioUtil.GetMp3Info(mp3FileName);
                Assert.True(mp3Info.DurationAsDouble > 3);
                File.Copy(mp3FileName, Path.Combine(@"c:\temp", $"{voiceName}.mp3"), true);
                OpenAI.Trace(new { voiceName, mp3Info }, this);
                AudioUtil.DeleteFile(mp3FileName);
            });
        }
    }

}