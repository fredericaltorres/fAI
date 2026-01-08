using DynamicSugar;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Cache;

namespace fAI
{
    /*
     
     https://dev.hume.ai/reference/text-to-speech-tts/synthesize-file

     */
    public class HumeAISpeech : HttpBase
    {
        const string _ttsUrl = "https://api.hume.ai/v0/tts/file";
        const string _getVoiceListUrl = "https://api.hume.ai/v0/tts/voices";

        public HumeAISpeech(int timeOut = -1, string openAiKey = null) : base(timeOut, openAiKey)
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
        public string DefaultMaleCustomVoice => "FREDERIC TORRES ENGLISH";

        protected override ModernWebClient InitWebClient(bool addJsonContentType = true, Dictionary<string, object> extraHeaders = null)
        {
            var mc = new ModernWebClient(_timeout);
            mc.AddHeader("X-Hume-Api-Key", $"{_key}");

            if (addJsonContentType)
                mc.AddHeader("Content-Type", "application/json")
                  .AddHeader("Accept", "application/json");

            if (extraHeaders != null)
            {
                foreach (var h in extraHeaders)
                    mc.AddHeader(h.Key, h.Value.ToString());
            }

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
                        text = input, voice = new Voice() { name = voiceName, provider = provider.ToString() }
                    }
                }
            };
            var wc = InitWebClient();
            var response = wc.POST(_ttsUrl, body.ToJson());
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

        public class HumeGetVoicesResponse
        {
            [JsonProperty("page_number")]
            public int PageNumber { get; set; }

            [JsonProperty("page_size")]
            public int PageSize { get; set; }

            [JsonProperty("total_pages")]
            public int TotalPages { get; set; }

            [JsonProperty("voices_page")]
            public List<VoicesPage> VoicesPage { get; set; }

            [JsonProperty("voice_tag_summary")]
            public VoiceTagSummary VoiceTagSummary { get; set; }

            public static HumeGetVoicesResponse FromJson(string json)
            {
                return JsonConvert.DeserializeObject<HumeGetVoicesResponse>(json);
            }
        }

        public class Accent
        {
            public int American { get; set; }
            public int British { get; set; }
            public int English { get; set; }

            [JsonProperty("Recieved Pronunciation")]
            public int RecievedPronunciation { get; set; }

            [JsonProperty("Black American")]
            public int BlackAmerican { get; set; }
            public int California { get; set; }
            public int Southern { get; set; }
            public int Cockney { get; set; }
            public int Texas { get; set; }
            public int Transatlantic { get; set; }

            [JsonProperty("Eastern European")]
            public int EasternEuropean { get; set; }
            public int Indian { get; set; }
            public int Midwest { get; set; }

            [JsonProperty("New York")]
            public int NewYork { get; set; }

            [JsonProperty("New Zealand")]
            public int NewZealand { get; set; }
            public int Standard { get; set; }
            public int Welsh { get; set; }
            public int Australian { get; set; }
            public int Canadian { get; set; }
            public int French { get; set; }
            public int Irish { get; set; }
            public int London { get; set; }
            public int Mexican { get; set; }
            public int Nigerian { get; set; }
            public int Pirate { get; set; }
            public int Scottish { get; set; }
            public int Transylvanian { get; set; }
            public int Yorkshire { get; set; }
        }

        public class Age
        {
        }

        public class Custom
        {
        }

        public class Gender
        {
        }

        public class Language
        {
            public int English { get; set; }
            public int German { get; set; }
            public int Spanish { get; set; }
        }

        public class Tags
        {
            public List<string> LANGUAGE { get; set; }
            public List<string> ACCENT { get; set; }
        }

        public class VoicesPage
        {
            public string id { get; set; }
            public string name { get; set; }
            public string provider { get; set; }
            public Tags tags { get; set; }
        }

        public class VoiceTagSummary
        {
            public Language language { get; set; }
            public Gender gender { get; set; }
            public Age age { get; set; }
            public Accent accent { get; set; }
            public Custom custom { get; set; }
        }

        public List<VoicesPage> GetVoices(Provider provider = Provider.HUME_AI, int pageSize = 100, bool loadAllPage = true, int pageNumber = 0)
        {
            OpenAI.Trace(new {}, this);
            var wc = InitWebClient();
            var body = new { provider = provider.ToString(), page_size = 100 };
            var response = wc.GET(_getVoiceListUrl+$"?provider={provider}&page_size={pageSize}&page_number={pageNumber}");
            if (response.Success)
            {
                OpenAI.Trace(response.Text, this);
                var voices = JsonConvert.DeserializeObject<HumeGetVoicesResponse>(response.Text);
                if (loadAllPage)
                {
                    for (var p = 1; p < voices.TotalPages; p++)
                    {
                        var otherVoicePages = GetVoices(provider: provider, pageSize: pageSize, loadAllPage : false, pageNumber: p);
                        voices.VoicesPage.AddRange(otherVoicePages);
                    }
                }
                return voices.VoicesPage;
            }
            else
            {
                OpenAI.TraceError(response.Text, this);
                OpenAI.TraceError(response.Exception.Message, this);
                throw new OpenAIAudioSpeechException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
            }
        }
    }
}

