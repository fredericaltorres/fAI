using Newtonsoft.Json;
using static fAI.SpeechToTextEngine;

namespace fAI
{
    public class SpeechToTextTranscriptionOptions
    {
        private static readonly string _defaultLanguageIsoCode = "en";
        [JsonProperty("metadata")]
        public string Metadata { get; set; }

        [JsonProperty("source_config")]
        public SourceConfig SourceConfig { get; set; } = new SourceConfig();

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        [JsonProperty("custom_vocabularies")]
        public SpeechToTextSentence[] CustomVocabularies { get; }
        [JsonProperty("language")]
        public string LanguageIsoCode { get; }

        [JsonProperty("skip_diarization")]
        public bool SkipDiarization { get; } = true;

        [JsonProperty("remove_atmospherics")]
        public bool RemoveAtmospherics { get; } = false;

        public SpeechToTextTranscriptionOptions(string languageIsoCode = null, string url = null)
        {
            LanguageIsoCode = languageIsoCode ?? _defaultLanguageIsoCode;
            RemoveAtmospherics = LanguageIsoCode == _defaultLanguageIsoCode;

            if (url != null)
            {
                SourceConfig = new SourceConfig { Url = url };
            }
        }
    }
}
