using Newtonsoft.Json;
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

        public OpenAISpeech(int timeOut = -1, string openAiKey = null) : base(timeOut, openAiKey)
        {
        }

        private string GetPayLoad(string text, string voice, string model)
        {
            return JsonConvert.SerializeObject(new {
                model = model,
                input = text,
                voice =  voice.ToString()
            });
        }

        public string Create(string input, string voice, string mp3FileName = null, string model = "gpt-4o-mini-tts") // "tts-1"
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

