using DynamicSugar;
using fAI;
using Markdig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Xunit;
using static fAI.HumeAISpeech;
using static fAI.OpenAICompletions;
using static fAI.OpenAICompletionsEx;
using static System.Net.Mime.MediaTypeNames;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class GenericAIAudio_UnitTests : OpenAIUnitTestsBase
    {
        public GenericAIAudio_UnitTests ()
        {
            OpenAI.TraceOn = true;
        }

        const string Text = @"
Physician Dr. Morrow analyzed Jane Smith's health issues on May 13, 2026.

Jane Smith is demonstrating gradual recovery from a right ankle ligament strain, with expected healing progression. Residual symptoms are consistent with ongoing ligament repair and mild inflammation.

The plan includes:
1. Continue use of ankle brace during physical activity.
2. Begin structured physical therapy program focused on strengthening ankle-stabilizing muscles and improving flexibility and range of motion.
3. Gradually reintroduce normal activities as tolerated.
4. Continue intermittent icing after activity if swelling occurs.
5. Consider MRI imaging if symptoms persist beyond the next 2–3 weeks

";

        [Fact()]
        [TestBeforeAfter]
        public void GenerateAudioWithHumeAI()
        {
            var ga = new GenericAIAudio();
            var voices = ga.GetVoices(GenericAIAudioProvider.HUME_AI);
            var mp3 = ga.Create(GenericAIAudioProvider.HUME_AI, Text, voices[voices.Keys.ToList()[0]]);
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenerateAudioWithOpenAI()
        {
            var ga = new GenericAIAudio();
            var voices = ga.GetVoices(GenericAIAudioProvider.OPEN_AI);
            var mp3 = ga.Create(GenericAIAudioProvider.OPEN_AI, Text, voices[voices.Keys.ToList()[0]]);
        }
    }
}