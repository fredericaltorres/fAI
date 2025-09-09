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
    public class HumeAIAudioTextToSpeech
    {
        const string input = @"Maybe I'm amazed at the way you pulled me out of time. Hung me on a line.";

        [Fact()]
        public void SpeechToText_DefaultMaleEnglishVoice()
        {
            var client = new HumeAI();
            var mp3FileName = client.Audio.Speech.Create(input, client.Audio.Speech.DefaultMaleEnglishVoice);

            var mp3Info = AudioUtil.GetMp3Info(mp3FileName);
            Assert.True(mp3Info.DurationAsDouble > 3);

            AudioUtil.DeleteFile(mp3FileName);
        }

        [Fact()]
        public void SpeechToText_DefaultMaleCustomVoice()
        {
            var client = new HumeAI();
            var mp3FileName = client.Audio.Speech.Create(input, client.Audio.Speech.DefaultMaleCustomVoice, provider: HumeAISpeech.Provider.CUSTOM_VOICE);

            var mp3Info = AudioUtil.GetMp3Info(mp3FileName);
            Assert.True(mp3Info.DurationAsDouble > 3);

            AudioUtil.DeleteFile(mp3FileName);
        }

        [Fact()]
        public void GetVoices_PageSize_100()
        {
            var client = new HumeAI();
            var voices = client.Audio.Speech.GetVoices();
            Assert.Equal(101, voices.Count);
        }

        [Fact()]
        public void GetVoices_PageSize_25()
        {
            var client = new HumeAI();
            var voices2 = client.Audio.Speech.GetVoices(pageSize: 25);
            Assert.Equal(101, voices2.Count);
        }
    }
}

