using System;
using Newtonsoft.Json;
using Brainshark.Cognitive.Library.ChatGPT;
using System.Net.Http.Headers;

namespace fAI
{
    public class GPT  
    {
        private int _timeout = 60 * 6;

        string _chatGPTKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        string _chatGPTOrg = Environment.GetEnvironmentVariable("OPENAI_ORGANIZATION_ID");

        public GPT()
        {
        }

        private static int CountWords(string str)
        {
            str = str.Trim();
            if (string.IsNullOrEmpty(str))
                return 0;

            string[] words = str.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }


        const string _chatGptTranslationUrl = "https://api.openai.com/v1/completions";
        private const string _chatGptTranslationModel = "gpt-3.5-turbo-instruct";


        //const string _chatGptSummarizationUrl   = "https://api.openai.com/v1/engines/davinci-003/completions";
        //const string _chatGptSummarizationModel = "text-davinci-003";

        const string _chatGptSummarizationUrl = _chatGptTranslationUrl;
        const string _chatGptSummarizationModel = _chatGptTranslationModel;

        const string _chatGptPromptUrl = "https://api.openai.com/v1/engines/davinci-codex/completions";

        // Azure.AI.OpenAI.
        // https://github.com/Azure-Samples/openai-dotnet-samples



        public class GPTPrompt
        {
            public string Url { get; set; }
            public string Text { get; set; }
            public string PrePrompt { get; set; }
            public string PostPrompt { get; set; }
            public string Prompt { get; set; }
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

        public class Prompt_GPT_35_TurboInstruct : GPTPrompt
        {
            public Prompt_GPT_35_TurboInstruct() : base ()
            {
                Model = "gpt-3.5-turbo-instruct";
                Url = "https://api.openai.com/v1/completions";
                MaxTokens = 2000;
                NewTokens = 400;
                Temperature = 0;
            }
        }

        public GPTPrompt Prompt(GPTPrompt p)
        {
            var webClient = InitWebClient();

            // For a translation we allow for max token usable by the model 3 times the number of words in the text
            var body = new
            {
                model =  p.Model,
                prompt = p.Prompt,
                max_tokens = p.MaxTokens,
                temperature = p.Temperature
            };
            var response = webClient.POST(_chatGptSummarizationUrl, p.GetPostBody()  /*JsonConvert.SerializeObject(body)*/);
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                p.Response = ChatGPTResponse.FromJson(response.Text);
                return p;
            }
            else throw new ChatGPTException($"{nameof(Translate)}() failed - {response.Exception.Message}", response.Exception);
        }

        public  string Summarize(string text, TranslationLanguages sourceLangague) 
        {
            var prompt = new Prompt_GPT_35_TurboInstruct 
            {
                Text = text,
                PrePrompt = "Summarize the following text: \n===\n",
                PostPrompt = "\n===\nSummary:\n",
            };

            //var body = new
            //{
            //    model = prompt.Model,
            //    prompt = prompt.FullPrompt,
            //    max_tokens = prompt.MaxTokens,
            //    temperature = prompt.Temperature
            //};
            var response = InitWebClient().POST(prompt.Url, prompt.GetPostBody() /*JsonConvert.SerializeObject(body)*/);
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                prompt.Response = ChatGPTResponse.FromJson(response.Text);
                if (prompt.Success)
                    return prompt.Answer;
                else
                    throw new ChatGPTException(prompt.Error);
            }
            else throw new ChatGPTException($"{nameof(Translate)}() failed - {response.Exception.Message}", response.Exception);
        }

        /// <summary>
        /// The max token for the translatio model is 4096. So we might have to split the text into multiple chunks
        /// </summary>
        /// <param name="text"></param>
        /// <param name="sourceLangague"></param>
        /// <param name="targetLanguage"></param>
        /// <returns></returns>
        /// <exception cref="ChatGPTException"></exception>

        public  string Translate(string text, TranslationLanguages sourceLangague, TranslationLanguages targetLanguage)
        {
            var webClient = InitWebClient();
            string prompt = $"Translate the following {sourceLangague} text to {targetLanguage}: '{text}'";

            // For a translation we allow for max token usable by the model 3 times the number of words in the text
            int maxTokens = CountWords(text) * 3;

            var body = new
            {
                model = _chatGptTranslationModel,
                prompt = prompt,
                max_tokens = maxTokens,
                temperature = 0
            };
            var response = webClient.POST(_chatGptTranslationUrl, JsonConvert.SerializeObject(body));

            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var chatGPTResponse = ChatGPTResponse.FromJson(response.Text);
                if (chatGPTResponse.Success)
                    return chatGPTResponse.Text;
                else
                    throw new ChatGPTException(chatGPTResponse.ErrorMessage);
            }
            else throw new ChatGPTException($"{nameof(Translate)}() failed - {response.Exception.Message}", response.Exception);
        }

        private ModernWebClient InitWebClient()
        {
            var mc = new ModernWebClient(_timeout);
            mc.AddHeader("Authorization", $"Bearer {_chatGPTKey}")
              .AddHeader("OpenAI-Organization", _chatGPTOrg)
              .AddHeader("Content-Type", "application/json")
              .AddHeader("Accept", "application/json");
            return mc;
        }
    }
}

