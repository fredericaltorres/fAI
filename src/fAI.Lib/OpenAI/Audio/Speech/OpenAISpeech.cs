using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Cache;

namespace fAI
{
    public class OpenAISpeech : HttpBase
    {
        const string __url = "https://api.openai.com/v1/audio/speech";

        public enum Voices
        {
            alloy,
            ash,
            ballad,
            coral,
            echo,
            fable,
            nova,
            onyx,
            sage,
            shimmer,
            verse,
        }

        public static List<string> VoicesAsString {
            get
            {
                var list = new List<string>();
                foreach (var v in (Voices[])System.Enum.GetValues(typeof(Voices)))
                    list.Add(v.ToString());
                return list;
            }
        }

        public OpenAISpeech(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }

        private string GetPayLoad(string text, string voice, string model, string instructions)
        {
            return JsonConvert.SerializeObject(new {
                model = model,
                input = text,
                voice =  voice.ToString(),
                instructions = instructions
            });
        }

        public static (string Left, string Right) SplitFromMiddleOnDot(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input string cannot be null or empty.", nameof(input));

            int middleIndex = input.Length / 2;

            int dotIndex = input.IndexOf('.', middleIndex);
            if (dotIndex == -1)
                throw new InvalidOperationException("No '.' found after the middle of the string.");

            string left = input.Substring(0, dotIndex);
            string right = input.Substring(dotIndex + 1);

            return (left, right);
        }

        const int OPEN_AI_MAX_TOKEN_FOR_SPEECH = 1900;

        public string Create(string input, string voice, string mp3FileName = null, 
            string model = "gpt-4o-mini-tts", string instructions = "Speak in a cheerful and positive tone.",
            int inputTokenCount = -1) // "tts-1"
        {
            if (mp3FileName == null)
                mp3FileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".mp3");

            if (inputTokenCount > OPEN_AI_MAX_TOKEN_FOR_SPEECH)
            {
                var (s1, s2) = SplitFromMiddleOnDot(input);
                var f1 = Create(s1, voice, mp3FileName: null, model, instructions);
                var f2 = Create(s2, voice, mp3FileName: null, model, instructions);
                var f1WavFile = AudioUtil.ConvertMp3ToWav(f1, f1 + ".wav");
                var f2WavFile = AudioUtil.ConvertMp3ToWav(f2, f2 + ".wav");
                var finalMp3 = AudioUtil.ConcatenateWavFiles(f1WavFile, f2WavFile , f1WavFile + ".concat.mp3", asMp3: true);
                File.Delete(f1);
                File.Delete(f2);
                File.Delete(f1WavFile);
                File.Delete(f2WavFile);
                File.Copy(finalMp3, mp3FileName, true);
                return mp3FileName;
            }

            OpenAI.Trace(new { input, voice, model }, this);

            var wc = InitWebClient();
            var response = wc.POST(__url, GetPayLoad(input, voice, model, instructions));
            if (response.Success)
            {
                var ext = wc.GetResponseImageExtension();
                File.WriteAllBytes(mp3FileName, response.Buffer);
                return mp3FileName;
            }
            else throw new OpenAIAudioSpeechException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
        }
    }
}

