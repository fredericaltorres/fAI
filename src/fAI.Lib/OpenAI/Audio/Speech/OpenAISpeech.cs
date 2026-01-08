using Newtonsoft.Json;
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
            echo,
            fable,
            onyx,
            nova,
            shimmer
        }
        public OpenAISpeech(int timeOut = -1, string openAiKey = null) : base(timeOut, openAiKey)
        {
        }

        private string GetPayLoad(string text, Voices voice, string model)
        {
            return JsonConvert.SerializeObject(new {
                model = model,
                input = text,
                voice =  voice.ToString()
            });
        }

        public string Create(string input, Voices voice, string mp3FileName = null, string model = "tts-1")
        {
            OpenAI.Trace(new { input, voice, model }, this);

            var wc = InitWebClient();
            var response = wc.POST(__url, GetPayLoad(input, voice, model));
            if (response.Success)
            {
                var ext = wc.GetResponseImageExtension();
                if (mp3FileName == null)
                    mp3FileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".mp3");

                File.WriteAllBytes(mp3FileName, response.Buffer);
                return mp3FileName;
            }
            else throw new OpenAIAudioSpeechException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
        }
    }
}

