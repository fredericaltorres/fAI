using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.GPT;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class MicrosoftCognitiveServicesTests
    {
        [Fact()]
        public void GetModels()
        {
            var text = @"I am he as you are he as you are me
And we are all together
See how they run like pigs from a gun
See how they fly
I'm crying";
            var mcs = new MicrosoftCognitiveServices();
            var voiceId = "en-US-DavisNeural";
            var mp3FileName = Path.Combine(Path.GetTempPath(), "mp3.mp3");
            mcs.CreateAudioFile(text, voiceId, mp3FileName);
          
        }
    }
}