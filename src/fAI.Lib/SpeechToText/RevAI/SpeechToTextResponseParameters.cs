using Newtonsoft.Json;
using System.Collections.Generic;

namespace fAI
{
    public class SpeechToTextResponseParameters
    {
        [JsonProperty("options.language")]
        public List<string> OptionsLanguage { get; set; }
    }
}
