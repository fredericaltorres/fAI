using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Cache;

namespace fAI
{
    public class HumeAISpeech : HttpBase
    {
        const string __url = "https://api.hume.ai/v0/tts/file";

        public HumeAISpeech(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut, openAiKey, openAiOrg)
        {
        }

        public class Format
        {
            public string type { get; set; }
        }

        public class HumeTTSBody
        {
            public List<Utterance> utterances { get; set; }
            public int num_generations { get; set; } = 1;
            public Format format { get; set; } = new Format() { type = "mp3" };

            public string ToJson()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public class Utterance
        {
            public string text { get; set; }
            public Voice voice { get; set; }
        }

        public class Voice
        {
            public string name { get; set; }
            public string provider { get; set; }
        }

        public enum Provider
        {
            HUME_AI,
            CUSTOM_VOICE
        }

        public string DefaultMaleEnglishVoice => "Male English Actor";

        protected override ModernWebClient InitWebClient(bool addJsonContentType = true)
        {
            var mc = new ModernWebClient(_timeout);
            mc.AddHeader("X-Hume-Api-Key", $"{_key}");

            if (addJsonContentType)
                mc.AddHeader("Content-Type", "application/json")
                  .AddHeader("Accept", "application/json");
            return mc;
        }

        public string Create(string input, string voiceName, string mp3FileName = null, Provider provider = Provider.HUME_AI)
        {
            OpenAI.Trace(new { input, voiceName }, this);

            var body = new HumeTTSBody()
            {
                utterances = new List<Utterance>()
                {
                    new Utterance()
                    {
                        text = input,
                        voice = new Voice()
                        {
                            name = voiceName,
                            provider = provider.ToString()
                        }
                    }
                }
            };
            var wc = InitWebClient();
            var response = wc.POST(__url, body.ToJson());
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

