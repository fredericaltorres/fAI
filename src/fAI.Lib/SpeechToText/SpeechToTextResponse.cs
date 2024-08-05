using Newtonsoft.Json;
using System;

namespace fAI
{
    public class SpeechToTextResponse
    {
        [JsonProperty("parameters")]
        public SpeechToTextResponseParameters Parameters { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_on")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("duration_seconds")]
        public double DurationSeconds { get; set; }

        public bool Success => Status == "transcribed" || Status == "completed";

        public bool InProgress => Status == "in_progress";

        public static SpeechToTextResponse FromJSON(string json)
        {
            return JsonUtils.FromJSON<SpeechToTextResponse>(json);
        }
    }
}
