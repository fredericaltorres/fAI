using DynamicSugar;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace fAI
{
    public enum MessageRole  {
        system,
        user, 
        assistant, 
        function
    }
    public class GPTMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "role")]
        public MessageRole Role { get; set; }

        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }


        public override string ToString()
        {
            return $"Role:{this.Role}, Content:{this.Content}";
        }
    }

    public class GPTPrompt
    {
        public ResponseFormat response_format { get; set; } = null;
        public string Url { get; set; }
        public string Text { get; set; }
        public List<GPTMessage> Messages { get; set; } = new List<GPTMessage>();

        public CompletionResponse Response { get; set; }
        public GPTPrompt UnprocessPrompt { get; set; } // allow to back up the current prompt before being processed.

        public GPTPrompt Clone()
        {
            var p = new GPTPrompt();
            p.response_format = this.response_format;
            p.Url = this.Url;
            p.Text = this.Text;
            p.Messages.AddRange(this.Messages);
            return p;
        }

        public void ProcessPrompt(Dictionary<string, object> parameters)
        {
            this.UnprocessPrompt = this.Clone();
            if(this.Messages.Count > 0)
            {
                this.Messages.ForEach(m => m.Content = m.Content.Template(parameters, "[", "]"));
            }
            else
            {
                this.Text = this.Text.Template(parameters, "[", "]");
            }
        }


        public string GetPromptString()
        {
            var sb = new System.Text.StringBuilder();
            foreach(var message in Messages)
            {
                sb.AppendLine($"{message.Role}: {message.Content}");
            }
            return sb.ToString();
        }

        public string PrePrompt { get; set; }
        public string PostPrompt { get; set; }
        public string Model { get; set; }
        public int MaxTokens { get; set; } = 4000;
        public int NewTokens { get; set; } = 500;

        const double DEFAULT_TEMPERATURE = 0.1;

        /*
            Low Temperature (Close to 0): At low temperatures, the model's responses are more deterministic and less random. This means it will choose the most likely next word or phrase based on its training. The responses tend to be more conservative and less varied. If you set the temperature very low, the model might repeat more predictable or common phrases.
            High Temperature (Closer to 1): At higher temperatures, the model introduces more randomness into its choices. This can lead to more creative, diverse, and sometimes unexpected responses. It's more likely to produce unique or less common outputs, but there's also a higher chance of nonsensical or irrelevant responses.
            Temperature Range: The temperature usually ranges from 0 to 1. A temperature of 0 means the model will always choose the most likely next word, while a temperature of 1 allows for maximum randomness in word selection.
            Use Cases: Lower temperatures are typically used when you want more accurate, reliable, and coherent responses. Higher temperatures are used when you want the model to generate more creative, diverse, or less obvious outputs.
            Balance: Finding the right temperature often involves balancing between creativity and coherence. A moderate temperature value (like 0.7) is a common choice for a mix of reliability and inventiveness in responses.
        */
        public double Temperature { get; set; } = DEFAULT_TEMPERATURE;

        public string FullPrompt
        {
            get
            {
                if(this.Messages.Count > 0)
                {
                    return string.Join("\r\n", this.Messages.Select(m => $"{m.Role}: {m.Content}"));
                }
                else
                {
                    return this.Text;
                }
            }
        }

        public override string ToString()
        {
            if(!string.IsNullOrEmpty(this.Text))
                return this.FullPrompt;
            return string.Join("\r\n", this.Messages);
        }

        

        public bool Success => Response.Success;

        public string Answer => (this.Response!= null && string.IsNullOrEmpty(this.Response.Text)) ? null : Response.Text.Trim();
        public string Error => Response.ErrorMessage;

        public string GetPostBody()
        {
            if (this.Messages != null && this.Messages.Count > 0)
            {
                return JsonConvert.SerializeObject(new
                {
                    model = Model,
                    messages = Messages,
                    max_tokens = MaxTokens,
                    temperature = Temperature,
                    response_format = response_format,
                });
            }
            else
            {
                return JsonConvert.SerializeObject(new
                {
                    model = Model,
                    prompt = FullPrompt,
                    max_tokens = MaxTokens,
                    temperature = Temperature
                });
            }
        }
    }
   
}

