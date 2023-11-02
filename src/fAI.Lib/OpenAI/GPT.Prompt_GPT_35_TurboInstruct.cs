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
            Temperature = 0;
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
            Temperature = 0;
        }
    }
}

