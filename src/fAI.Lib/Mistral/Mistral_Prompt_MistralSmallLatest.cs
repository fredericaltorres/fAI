using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace fAI
{
    public class MistralMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "role")]
        public MessageRole Role { get; set; }

        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }

        public override string ToString()
        {
            return $"Role:{this.Role}, Content:{this.Content} ";
        }

        public MistralMessage()
        {
        }

        public MistralMessage(MessageRole role, string content)
        {
            this.Content = content;
            Role = role;
        }
    }

    // https://docs.mistral.ai/api/#tag/chat/operation/chat_completion_v1_chat_completions_post
    // https://github.com/tghamm/Mistral.SDK
    public class MistralPromptBase
    {
        public string Url { get; set; }
        public List<global::Mistral.SDK.DTOs.ChatMessage> Messages { get; set; } = new List<global::Mistral.SDK.DTOs.ChatMessage>();
        public string Model { get; set; }
        public int MaxTokens { get; set; } = 2048;
        public int RandomSeed { get; set; } = 0;
        public int TopP { get; set; } = 1;
        public bool Stream { get; set; } = false;
        public bool SafePrompt { get; set; } = true;
        public double Temperature { get; set; } = 0;
        public bool _responseFormatAsJson = false;

        public virtual string GetPostBody() // Just used for display since we use the Mistral SDK
        {
            if (this.Messages != null && this.Messages.Count > 0)
                return JsonConvert.SerializeObject(this);
            else 
                throw new System.Exception("No messages to send");
        }

        public MistralPromptBase(string model = "mistral-small-latest")
        {
            Model = model;
            Url = "https://api.mistral.ai/v1/chat/completions";
        }

        public void SetResponseFormatAsJson()
        {
            _responseFormatAsJson = true;
        }
    }

    public class Mistral_Prompt_Codestral : MistralPromptBase
    {
        public Mistral_Prompt_Codestral() : base()
        {
            Model = global::Mistral.SDK.ModelDefinitions.MistralMedium;
            base.Url = "https://api.mistral.ai/v1/codestral/completions";
        }
    }

    // https://github.com/tghamm/Mistral.SDK
    public class Mistral_Prompt_MistralSmallLatest : MistralPromptBase
    {
        public Mistral_Prompt_MistralSmallLatest() : base()
        {
            Model = global::Mistral.SDK.ModelDefinitions.MistralMedium;
        }
    }
}

