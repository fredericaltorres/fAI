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

    public class AnthropicPrompt
    {
        public string Url { get; set; }
        public List<AnthropicMessage> Messages { get; set; } = new List<AnthropicMessage>();
        public string Model { get; set; }
        public int MaxTokens { get; set; } = 4000;
        public string System { get; set; }

        public virtual string GetPostBody()
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
    // https://docs.anthropic.com/en/prompt-library
    public class Anthropic_Prompt_Claude_3_Opus : AnthropicPrompt
    {
        public Anthropic_Prompt_Claude_3_Opus() : base()
        {
            Model = "claude-3-opus-20240229";
            MaxTokens = 1024;
            Url = "https://api.anthropic.com/v1/messages";
        }

        //public override string GetPostBody()
        //{
        //    return null;
        //}
    }

    public class Anthropic_Image_Prompt_Claude_3_Opus : AnthropicPrompt
    {
        public Anthropic_Image_Prompt_Claude_3_Opus() : base()
        {
            Model = "claude-3-opus-20240229";
            MaxTokens = 1024;
            Url = "https://api.anthropic.com/v1/messages";
        }

        //public override string GetPostBody()
        //{
        //    if (this.Messages != null && this.Messages.Count > 0)
        //    {
        //        return JsonConvert.SerializeObject(new
        //        {
        //            model = Model,
        //            messages = this.Messages,
        //            max_tokens = MaxTokens,
        //        });
        //    }
        //    else throw new System.Exception("No messages to send");
        //}
    }
}

