using Newtonsoft.Json;

namespace fAI
{
    public class GPTPrompt
    {
        public string Url { get; set; }
        public string Text { get; set; }
        public string PrePrompt { get; set; }
        public string PostPrompt { get; set; }
        //public string Prompt { get; set; }
        public string Model { get; set; }
        public int MaxTokens { get; set; } = 4000;
        public int NewTokens { get; set; } = 500;
        public int Temperature { get; set; } = 0;

        public string FullPrompt => $"{PrePrompt}{Text}{PostPrompt}";

        public ChatGPTResponse Response { get; set; } = new ChatGPTResponse();

        public bool Success => Response.Success;

        public string Answer => string.IsNullOrEmpty(Response.Text) ? null : Response.Text.Trim();
        public string Error => Response.ErrorMessage;

        public string GetPostBody()
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

