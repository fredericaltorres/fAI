using DynamicSugar;
using MimeTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace fAI
{

    public enum AnthropicContentMessageType
    {
        text,
        image
    }

    public class AnthropicContentMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "type")]
        public AnthropicContentMessageType Type { get; set; } = AnthropicContentMessageType.text;
    }

    public class AnthropicContents : List<AnthropicContentMessage>
    {
        public AnthropicContents()
        {
        }

        public AnthropicContents(params AnthropicContentMessage[] messages)
        {
            this.AddRange(messages);
        }
    }

    public class AnthropicContentMessageSource : AnthropicContentMessage
    {
        [JsonProperty(PropertyName = "source")]
        public Dictionary<string, object> Source { get; set; }
    }

    public class AnthropicContentImage : AnthropicContentMessageSource
    {
        public AnthropicContentImage(string imageFileName)
        {
            Type = AnthropicContentMessageType.image;
            Source = DS.Dictionary(new
            {
                type = "base64",
                media_type = MimeTypeMap.GetMimeType(imageFileName),
                data = Convert.ToBase64String(System.IO.File.ReadAllBytes(imageFileName))
            });
        }
    }

    public class AnthropicContentText : AnthropicContentMessage
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        public AnthropicContentText()
        {

        }
        public AnthropicContentText(string text)
        {
            Text = text;
        }

        public override string ToString()
        {
            return $"Text:{this.Text}";
        }
    }

    public class AnthropicMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "role")]
        public MessageRole Role { get; set; }

        [JsonProperty(PropertyName = "content")]
        public List<AnthropicContentMessage> Content { get; set; }

        public override string ToString()
        {
            return $"Role:{this.Role}, Content:{this.Content.Count} Contents";
        }

        public AnthropicMessage()
        {
        }

        public AnthropicMessage(MessageRole role, List<AnthropicContentMessage> content)
        {
            this.Content = content.ToList();
            Role = role;
        }

        public AnthropicMessage(MessageRole role, params AnthropicContentMessage[] content)
        {
            this.Content = content.ToList();
            Role = role;
        }
    }

    public class AnthropicMessages : List<AnthropicMessage>
    {
        public AnthropicMessages()
        {
        }

        public AnthropicMessages(params AnthropicMessage[] messages)
        {
            this.AddRange(messages);
        }
    }

    public class AnthropicPromptBase
    {
        public string Url { get; set; }
        public List<AnthropicMessage> Messages { get; set; } = new List<AnthropicMessage>();
        public string Model { get; set; }
        public int OutputMaxTokens { get; set; } = 1024*4;
        public int InputMaxTokens { get; set; } = 200000;
        public string System { get; set; } = null;
        public int Temperature { get; set; } = 1;

        public virtual string GetPostBody()
        {
            if (this.Messages != null && this.Messages.Count > 0)
            {
                if(this.System == null)
                {
                    return JsonConvert.SerializeObject(new { model = Model, messages = this.Messages,max_tokens = OutputMaxTokens, temperature = Temperature });
                }
                else
                {
                    return JsonConvert.SerializeObject(new { system = System, model = Model, messages = this.Messages, max_tokens = OutputMaxTokens, temperature = Temperature });
                }
            }
            else throw new System.Exception("No messages to send");
        }

        public AnthropicPromptBase(string model = "claude-3-opus-20240229") // Make sure we clone all property
        {
            Model = "claude-3-opus-20240229";
            Url = "https://api.anthropic.com/v1/messages";
        }

        public string FullPrompt
        {
            get
            {
                var sb = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(System))
                    sb.AppendLine($"System: {System}");

                if (this.Messages.Count > 0)
                {
                    foreach(var m in this.Messages)
                    {
                        sb.Append($"{m.Role}: ");
                        foreach(var c in m.Content)
                        {
                            if(c is AnthropicContentText textContent)
                                sb.AppendLine($"{textContent.Text}");
                            else if (c is AnthropicContentMessage textMessage)
                                sb.AppendLine($"(Type: {textMessage.Type})"); // Should not happen, but just in case.
                            else if(c is AnthropicContentImage imageContent)
                                sb.AppendLine($"  Image: {imageContent.Source["media_type"]} (base64 data)");
                        }
                    }
                }
                else
                {
                    sb.Append("No messages in prompt.");
                }
                return sb.ToString();
            }
        }
    }

    public class Anthropic_Prompt_Claude_3_5_Sonnet : AnthropicPromptBase
    {
        public Anthropic_Prompt_Claude_3_5_Sonnet() : base()
        {
            Model = "claude-3-5-sonnet-20241022";
            
        }
    }

    public class Anthropic_Prompt_Claude_4_Opus : AnthropicPromptBase
    {
        public Anthropic_Prompt_Claude_4_Opus() : base()
        {
            Model = "claude-opus-4-20250514";
            OutputMaxTokens = 32000;
            Url = "https://api.anthropic.com/v1/messages";
        }
    }

    // https://docs.anthropic.com/en/docs/
    // https://console.anthropic.com/dashboard API KEY
    // https://github.com/anthropics/anthropic-cookbook
    // https://github.com/anthropics/anthropic-cookbook/blob/main/multimodal/getting_started_with_vision.ipynb
    // https://docs.anthropic.com/en/prompt-library
    public class Anthropic_Prompt_Claude_3_Opus : AnthropicPromptBase
    {
        public Anthropic_Prompt_Claude_3_Opus() : base()
        {
            Model = "claude-3-opus-20240229";
            Url = "https://api.anthropic.com/v1/messages";
        }
    }

    public class Anthropic_Image_Prompt_Claude_3_Opus : AnthropicPromptBase
    {
        public Anthropic_Image_Prompt_Claude_3_Opus() : base()
        {
            Model = "claude-3-opus-20240229";
            Url = "https://api.anthropic.com/v1/messages";
        }
    }
}

