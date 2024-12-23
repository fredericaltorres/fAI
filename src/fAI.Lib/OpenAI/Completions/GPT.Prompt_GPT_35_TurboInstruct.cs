using Newtonsoft.Json;
using System.Linq;

namespace fAI
{
    public class Prompt_GPT_35_TurboInstruct : GPTPrompt
    {
        public Prompt_GPT_35_TurboInstruct() : base()
        {
            Model = "gpt-3.5-turbo-instruct";
            Url = GPTPrompt.OPENAI_URL_V1_COMPLETIONS;
        }
    }

    public class Prompt_GPT_35_Turbo : GPTPrompt
    {
        public Prompt_GPT_35_Turbo() : base()
        {
            Model = "gpt-3.5-turbo";
            Url = GPTPrompt.OPENAI_URL_V1_CHAT_COMPLETIONS;
        }
    }

    //public class Prompt_GPT_35_DaVinci : GPTPrompt
    //{
    //    public Prompt_GPT_35_DaVinci() : base()
    //    {
    //        Model = "davinci-002";
    //        Url = "https://api.openai.com/v1/engines/davinci-002/completions";
    //    }
    //}

    public class JsonResponseFormat
    {
        public string type { get; set; } = "json_object";
    }

    public class Prompt_GPT_35_Turbo_JsonAnswer : GPTPrompt
    {
        public Prompt_GPT_35_Turbo_JsonAnswer() : base()
        {
            Model = "gpt-3.5-turbo-1106";
            Url = GPTPrompt.OPENAI_URL_V1_CHAT_COMPLETIONS;
            response_format = new JsonResponseFormat();
        }
    }

    public class Prompt_GPT_4 : GPTPrompt
    {
        public Prompt_GPT_4() : base()
        {
            Model = "gpt-4";
            Url = GPTPrompt.OPENAI_URL_V1_CHAT_COMPLETIONS;
        }
    }

    public class Prompt_GPT_3_5_CodeGeneration : GPTPrompt
    {
        public Prompt_GPT_3_5_CodeGeneration() : base()
        {
            Model = "gpt-3.5-turbo-instruct";
            Url = GPTPrompt.OPENAI_URL_V1_COMPLETIONS;
        }
    }

    public class Groq_Prompt_Mistral : GPTPrompt
    {
        public Groq_Prompt_Mistral() : base()
        {
            Model = "mixtral-8x7b-32768";
            Url = "https://api.groq.com/openai/v1/chat/completions";
        }
    }

    // https://docs.anthropic.com/en/docs/
    // https://console.anthropic.com/dashboard API KEY
    // https://github.com/anthropics/anthropic-cookbook
    // https://github.com/anthropics/anthropic-cookbook/blob/main/multimodal/getting_started_with_vision.ipynb
    public class Anthropic_Prompt_Claude_3_Opus : GPTPrompt
    {
        public Anthropic_Prompt_Claude_3_Opus() : base()
        {
            Model = "claude-3-opus-20240229";
            MaxTokens = 1024;
            Url = "https://api.anthropic.com/v1/messages";
        }

        public override string GetPostBody()
        {
            if (this.Messages != null && this.Messages.Count > 0)
            {
                // With Anthropic, we need to send the system message as a specific field
                var systemMessage = string.Empty;
                var systemMessages = this.Messages.Where(m => m.Role == MessageRole.system).ToList();
                if (systemMessages.Count > 0)
                {
                    systemMessage = systemMessages[0].Content;
                }

                var nonSystemMessages = this.Messages.Where(m => m.Role != MessageRole.system).ToList();

                return JsonConvert.SerializeObject(new
                {
                    system = string.IsNullOrEmpty(systemMessage) ? null : systemMessage,
                    model = Model,
                    messages = nonSystemMessages,
                    max_tokens = MaxTokens,
                    temperature = Temperature,
                });
            }
            else throw new System.Exception("No messages to send");
        }
    }

    public class Anthropic_Image_Prompt_Claude_3_Opus : AnthropicPrompt
    {
        public Anthropic_Image_Prompt_Claude_3_Opus() : base()
        {
        }

        public string GetPostBody()
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
