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

        const string TextSample = @"
Win Speak is a Windows Dictation Application which

- Used speech recognition technology to convert spoken words into Text with high accuracy and speed.
- Then using AI LLM the application improves the Text recorded in term of grammar, punctuation, and context.
- The Windows App run in background, therefore the improved Text is sent automatically in the application with the focus.
- Win Speak can also applied to the Text extracted the following actions
    * Summarization.
    * Title generation.
    * Bullet point generation.
    * Translate the Text generated into another language.
    * Publish the Text generated and it's audio representation via an URL.
";

        [Fact()]
        public void OpenAI_Voices_Samples_Generator()
        {
            var client = new OpenAI();
            OpenAISpeech.VoicesAsString.ToList().ForEach(voiceName =>
            {
                var mp3FileName = client.Audio.Speech.Create(TextSample, voiceName
                    //, instructions : "Speak like a drunken pirate."
                    );
                var mp3Info = AudioUtil.GetMp3Info(mp3FileName);
                Assert.True(mp3Info.DurationAsDouble > 3);
                File.Copy(mp3FileName, Path.Combine(@"c:\temp\openai", $"{voiceName}.WinSpeak.mp3"), true);
                OpenAI.Trace(new { voiceName, mp3Info }, this);
                AudioUtil.DeleteFile(mp3FileName);
            });
        }


        [Fact()]
        public void OpenAI_Voices_Samples_Generator_SlicedIn2Files()
        {
            var client = new OpenAI();
            var voiceName = OpenAISpeech.VoicesAsString[0];
            var mp3FileName = client.Audio.Speech.Create(TextSample, voiceName, inputTokenCount: 5000 /* Force to create the mp3 in 2 operations */);
            var mp3Info = AudioUtil.GetMp3Info(mp3FileName);
            Assert.True(mp3Info.DurationAsDouble > 3);
            OpenAI.Trace(new { voiceName, mp3Info }, this);
            AudioUtil.DeleteFile(mp3FileName);
        }
    }
}