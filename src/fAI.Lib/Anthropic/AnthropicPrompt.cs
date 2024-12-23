using Newtonsoft.Json;
using System.Collections.Generic;

namespace fAI
{
    public class AnthropicPrompt
    {
        public string Url { get; set; }
        public List<AnthropicMessage> Messages { get; set; } = new List<AnthropicMessage>();
        public string Model { get; set; }
        public int MaxTokens { get; set; } = 4000;

        public virtual string GetPostBody()
        {
            return null;
        }

        public AnthropicPrompt() // Make sure we clone all property
        {
            Model = "claude-3-opus-20240229";
            MaxTokens = 1024;
            Url = "https://api.anthropic.com/v1/messages";
        }
    }


    // https://docs.anthropic.com/en/docs/
    // https://console.anthropic.com/dashboard API KEY
    // https://github.com/anthropics/anthropic-cookbook
    // https://github.com/anthropics/anthropic-cookbook/blob/main/multimodal/getting_started_with_vision.ipynb
    public class Anthropic_Prompt_Claude_3_Opus : AnthropicPrompt
    {
        public Anthropic_Prompt_Claude_3_Opus() : base()
        {
            Model = "claude-3-opus-20240229";
            MaxTokens = 1024;
            Url = "https://api.anthropic.com/v1/messages";
        }

        public override string GetPostBody()
        {
            return null;
        }
    }

    public class Anthropic_Image_Prompt_Claude_3_Opus : AnthropicPrompt
    {
        public Anthropic_Image_Prompt_Claude_3_Opus() : base()
        {
            Model = "claude-3-opus-20240229";
            MaxTokens = 1024;
            Url = "https://api.anthropic.com/v1/messages";
        }

        public override string GetPostBody()
        {
            if (this.Messages != null && this.Messages.Count > 0)
            {

                return JsonConvert.SerializeObject(new
                {
                    model = Model,
                    messages = this.Messages,
                    max_tokens = MaxTokens,
                });
            }
            else throw new System.Exception("No messages to send");
        }
    }
}

