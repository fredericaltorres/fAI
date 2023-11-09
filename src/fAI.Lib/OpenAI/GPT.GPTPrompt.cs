using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

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
        public string Url { get; set; }
        public string Text { get; set; }

        public List<GPTMessage> Messages { get; set; } 

        public string PrePrompt { get; set; }
        public string PostPrompt { get; set; }
        public string Model { get; set; }
        public int MaxTokens { get; set; } = 4000;
        public int NewTokens { get; set; } = 500;
        public double Temperature { get; set; } = 0;

        public string FullPrompt => $"{PrePrompt}{Text}{PostPrompt}";

        public CompletionResponse Response { get; set; } = new CompletionResponse();

        public bool Success => Response.Success;

        public string Answer => (this.Response!= null && string.IsNullOrEmpty(this.Response.Text)) ? null : Response.Text.Trim();
        public string Error => Response.ErrorMessage;

        public string GetPostBody()
        {
            if (this.Messages != null)
            {
                return JsonConvert.SerializeObject(new
                {
                    model = Model,
                    messages = Messages,
                    max_tokens = MaxTokens,
                    temperature = Temperature
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

