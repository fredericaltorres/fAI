using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace fAI.AnthropicLib
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class CacheCreation
    {
        public int ephemeral_5m_input_tokens { get; set; }
        public int ephemeral_1h_input_tokens { get; set; }
    }

    public class Content
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        public bool IsText => Type == "text";
    }

    public class AnthropicCompletionResponse : BaseHttpResponse
    {
        [JsonProperty(PropertyName = "model")]
        public string Model { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "content")]
        public List<Content> Content { get; set; }

        [JsonProperty(PropertyName = "stop_reason")]
        public string StopReason { get; set; }

        [JsonProperty(PropertyName = "stop_sequence")]
        public object StopSequence { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public Usage Usage { get; set; }

        public static AnthropicCompletionResponse FromJson(string json) => Newtonsoft.Json.JsonConvert.DeserializeObject<AnthropicCompletionResponse>(json);

        public override string Text => this.Content.FirstOrDefault(c => c.IsText).Text;
    }

    public class Usage
    {
        public int input_tokens { get; set; }
        public int cache_creation_input_tokens { get; set; }
        public int cache_read_input_tokens { get; set; }
        public CacheCreation cache_creation { get; set; }
        public int output_tokens { get; set; }
        public string service_tier { get; set; }
    }
}
