using fAI.Util.Strings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fAI.AnthropicLib
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class CacheCreation
    {
        public int ephemeral_5m_input_tokens { get; set; }
        public int ephemeral_1h_input_tokens { get; set; }
    }

    public class ContentCaller
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }

    //public class Content
    //{
    //    [JsonProperty(PropertyName = "id")]
    //    public string Id { get; set; }

    //    [JsonProperty(PropertyName = "type")]
    //    public string Type { get; set; }

    //    [JsonProperty(PropertyName = "text")]
    //    public string Text { get; set; }

    //    public bool IsText => Type == "text";
    //    public bool IsToolUse => Type == "tool_use";

    //    // In case of tool calls, there will be a name property with the name of the tool being called. This is useful for tools that return text, but we want to know which tool was called.
    //    [JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
    //    public string Name { get; set; }

    //    [JsonProperty(PropertyName = "input", NullValueHandling = NullValueHandling.Ignore)]
    //    public Dictionary<string, object> Input { get; set; }

    //    [JsonProperty(PropertyName = "caller", NullValueHandling = NullValueHandling.Ignore)]
    //    public ContentCaller Caller { get; set; }
    //}

    public class Contents : List<AnthropicContentMessage>
    {
        public AnthropicContentMessage FindToolUse()
        {
            return this.FirstOrDefault(c => c.IsToolUse);
        }
        public AnthropicContentMessage FindToolByName(string name)
        {
            return this.FirstOrDefault(c => c.Name == name);
        }
    }

    public class AnthropicCompletionResponse : BaseHttpResponse
    {
        [JsonProperty(PropertyName = "model")]
        public string Model { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        public bool IsToolUse => this.StopReason == "tool_use";

        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "content")]
        public Contents Content { get; set; }

        [JsonProperty(PropertyName = "stop_reason")]
        public string StopReason { get; set; }

        [JsonProperty(PropertyName = "stop_sequence")]
        public object StopSequence { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public Usage Usage { get; set; }

        public static AnthropicCompletionResponse FromJson(string json) => Newtonsoft.Json.JsonConvert.DeserializeObject<AnthropicCompletionResponse>(json);

        public override string Text 
        { 
            get {
                var o = this.Content.FirstOrDefault(c => c.IsText);
                if(o == null)
                    return null;
                return o.Text;
            }
        }
        public override string AsJson => StringUtil.SmartExtractJson(this.Text);
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
