using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.AudioUtil;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class MicrosoftCognitiveServicesTests
    {
        const string EnglishTest01 = @"I am he as you are he as you are me
And we are all together.
See how they run like pigs from a gun
See how they fly.
I'm crying";


        const string STT_EnglishTest01_Result = "I am he as you are he as you are me. And we are all together. See how they run like pigs from a gun? See how they fly. I am crying.";

        [Fact()]
        public void ExecuteTTS()
        {
            var mcs = new MicrosoftCognitiveServices();
            var voiceId = "en-GB-LibbyNeural";
            var mp3FileName = Path.Combine(Path.GetTempPath(), "mp3.mp3");

            mcs.ExecuteTTS(EnglishTest01, voiceId, mp3FileName);

            Assert.True(File.Exists(mp3FileName));
            var mp3Info = AudioUtil.GetMp3Info(mp3FileName);
            Assert.True(mp3Info.DurationAsDouble > 3);
            Assert.Equal(48000, mp3Info.SampleRate);
            Assert.Equal(16, mp3Info.BitsPerSample);

            var sttResult = mcs.ExecuteSTT(mp3FileName);

            AudioUtil.DeleteFile(mp3FileName);
        }

        [Fact()]
        public void ExecuteSTT()
        {
            var mcs = new MicrosoftCognitiveServices();
            var mp3FileName = Path.Combine(".", "TestFiles", "TestFile.01.48Khz.mp3");

            var sttResult = mcs.ExecuteSTT(mp3FileName).GetAwaiter().GetResult();

            Assert.True(sttResult.Succeeded);
            Assert.Equal(STT_EnglishTest01_Result, sttResult.Text);
        }
    }
}