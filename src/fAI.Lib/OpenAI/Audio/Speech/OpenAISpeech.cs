using Newtonsoft.Json;
using System.Drawing;
using System.IO;
using System.Net.Cache;

namespace fAI
{
    public class OpenAISpeech : OpenAIHttpBase
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
        public OpenAISpeech(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut, openAiKey, openAiOrg)
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

        public string Create(string input, Voices voice, string model = "tts-1")
        {
            OpenAI.Trace(new { input, voice, model }, this);

            var wc = InitWebClient();
            var response = wc.POST(__url, GetPayLoad(input, voice, model));
            if (response.Success)
            {
                var ext = wc.GetResponseImageExtension();
                var tmpMp3FileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".mp3");
                File.WriteAllBytes(tmpMp3FileName, response.Buffer);
                return tmpMp3FileName;
            }
            else throw new OpenAIAudioSpeechException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
        }
    }
}

