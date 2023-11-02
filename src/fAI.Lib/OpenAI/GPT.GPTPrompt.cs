using Newtonsoft.Json;
using System.Collections.Generic;

namespace fAI
{
    public class GPTMessage
    {
        public string role { get; set; }
        public string content { get; set; }
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

        public ChatGPTResponse Response { get; set; } = new ChatGPTResponse();

        public bool Success => Response.Success;

        public string Answer => string.IsNullOrEmpty(Response.Text) ? null : Response.Text.Trim();
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

