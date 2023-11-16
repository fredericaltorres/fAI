namespace fAI
{
    
    public class Prompt_GPT_35_TurboInstruct : GPTPrompt
    {
        public Prompt_GPT_35_TurboInstruct() : base()
        {
            Model = "gpt-3.5-turbo-instruct";
            Url = "https://api.openai.com/v1/completions";
            MaxTokens = 2000;
            NewTokens = 400;
        }
    }

    public class Prompt_GPT_35_Turbo : GPTPrompt
    {
        public Prompt_GPT_35_Turbo() : base()
        {
            Model = "gpt-3.5-turbo";
            Url = "https://api.openai.com/v1/chat/completions";
            MaxTokens = 2000;
            NewTokens = 400;
        }
    }


    public class ResponseFormat
    {
        public string type { get; set; } = "json_object";
    }

    public class Prompt_GPT_35_Turbo_JsonMode : GPTPrompt
    {
        
        public Prompt_GPT_35_Turbo_JsonMode() : base()
        {
            Model = "gpt-3.5-turbo-1106";
            Url = "https://api.openai.com/v1/chat/completions";
            MaxTokens = 2000;
            NewTokens = 400;
            response_format = new ResponseFormat();
        }
    }

    public class Prompt_GPT_4  : GPTPrompt
    {
        public Prompt_GPT_4() : base()
        {
            Model = "gpt-4";
            Url = "https://api.openai.com/v1/chat/completions";

            MaxTokens = 2000;
            NewTokens = 400;
        }
    }
}

