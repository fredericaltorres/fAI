using Deepgram.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace fAI
{
    public class WhisperWord
    {
        [JsonProperty("word")]
        public string Value { get; set; }

        [JsonProperty("start")]
        public double Start { get; set; }

        [JsonProperty("end")]
        public double End { get; set; }
    }

    public class WhipserSpeechToTextResponse
    {
        [JsonProperty("task")]
        public string Task { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("words")]
        public List<WhisperWord> Words { get; set; }

        public string Status { get; set; }

        public bool Success => Status == "transcribed" || Status == "completed";

        public bool InProgress => Status == "in_progress";

        public static WhipserSpeechToTextResponse FromJSON(string json)
        {
            var r = JsonUtils.FromJSON<WhipserSpeechToTextResponse>(json);
            r.Status = "completed";
            return r;
        }
    }
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
