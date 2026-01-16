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

        [Fact()]
        public void OpenAI_Voices_Samples_Generator()
        {
            const string input = @"
Win Speak is a Windows Dictation Application which

- Used speech recognition technology to convert spoken words into text with high accuracy and speed.
- Then using AI LLM the application improves the text recorded in term of grammar, punctuation, and context.
- The Windows App run in background, therefore the improved text is sent automatically in the application with the focus.
- Win Whisper can also applied to the text extracted the following actions
    * Summarization.
    * Title generation.
    * Bullet point generation.
    * Translate the text generated into another language.
    * Publish the text generated and it's audio representation via an URL.
";
            var client = new OpenAI();

            OpenAISpeech.VoicesAsString.ToList().ForEach(voiceName =>
            {
                var mp3FileName = client.Audio.Speech.Create(input, voiceName
                    //, instructions : "Speak like a drunken pirate."
                    );
                var mp3Info = AudioUtil.GetMp3Info(mp3FileName);
                Assert.True(mp3Info.DurationAsDouble > 3);
                File.Copy(mp3FileName, Path.Combine(@"c:\temp\openai", $"{voiceName}.WinSpeak.mp3"), true);
                OpenAI.Trace(new { voiceName, mp3Info }, this);
                AudioUtil.DeleteFile(mp3FileName);
            });
        }
    }
}