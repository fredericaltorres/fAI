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

    public class Prompt_GPT_4  : GPTPrompt
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

    public class Anthropic_Prompt_Claude_3_Opus : GPTPrompt
    {
        public Anthropic_Prompt_Claude_3_Opus() : base()
        {
            Model = "claude-3-opus-20240229";
            _MaxTokens = 1024;
            Url = "https://api.anthropic.com/v1/messages";
        }
    }
}

