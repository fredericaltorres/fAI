using System;
using Newtonsoft.Json;
using Brainshark.Cognitive.Library.ChatGPT;

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

        // const string _chatGptSummarizationUrl = "https://api.openai.com/v1/engines/davinci-codex/completions"; 404
                                                 
        const string _chatGptSummarizationUrl   = "https://api.openai.com/v1/engines/davinci-003/completions";
        // const string _chatGptSummarizationUrl = "https://api.openai.com/v1/engines/davinci/completions";

        private const string _chatGptSummarizationModel = "text-davinci-003";

        const string _chatGptPromptUrl = "https://api.openai.com/v1/engines/davinci-codex/completions";

        // Azure.AI.OpenAI.
        // https://github.com/Azure-Samples/openai-dotnet-samples

        public string Prompt(string text)
        {
            var webClient = InitWebClient();
            string prompt = text;
            int maxTokens = CountWords(text) * 3;
            var body = new
            {
                model = _chatGptPromptUrl,
                max_tokens = maxTokens,
                temperature = 0
            };
            var response = webClient.POST(_chatGptSummarizationUrl, JsonConvert.SerializeObject(body));

            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var chatGPTResponse = ChatGPTResponse.FromJson(response.Text);
                if (chatGPTResponse.Success)
                    return chatGPTResponse.TranslatedText;
                else
                    throw new ChatGPTException(chatGPTResponse.ErrorMessage);
            }
            else throw new ChatGPTException($"{nameof(Translate)}() failed - {response.Exception.Message}", response.Exception);
        }

        public  string Summarize(string text, TranslationLanguages sourceLangague) 
        {
            const string PRE_PROMPT = "Summarize the following text: \n===\n";
            const string POST_PROMPT = "\n===\nSummary:\n";
            const int MAX_TOKENS = 4000;
            const int MAX_NEW_TOKENS = 500;

            var webClient = InitWebClient();
            string prompt = $"{PRE_PROMPT}{text}{POST_PROMPT}";

            // For a translation we allow for max token usable by the model 3 times the number of words in the text
            var body = new
            {
                model = _chatGptSummarizationModel,
                prompt = prompt,
                max_tokens = MAX_NEW_TOKENS,
                temperature = 0
            };
            var response = webClient.POST(_chatGptSummarizationUrl, JsonConvert.SerializeObject(body));

            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var chatGPTResponse = ChatGPTResponse.FromJson(response.Text);
                if (chatGPTResponse.Success)
                    return chatGPTResponse.TranslatedText;
                else
                    throw new ChatGPTException(chatGPTResponse.ErrorMessage);
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
                    return chatGPTResponse.TranslatedText;
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

