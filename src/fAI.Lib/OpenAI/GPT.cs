using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace fAI
{
    public partial class GPT  
    {
        private int _timeout = 60 * 2;

        string _chatGPTKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        string _chatGPTOrg = Environment.GetEnvironmentVariable("OPENAI_ORGANIZATION_ID");

        public GPT(int timeOut = -1)
        {
            if(timeOut > 0)
                _timeout = timeOut;
        }

        private static int CountWords(string str)
        {
            str = str.Trim();
            if (string.IsNullOrEmpty(str))
                return 0;

            string[] words = str.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }

        const string OpenAi_ModelUrl = "https://api.openai.com/v1/models";

        public Models GetModels()
        {
            var response = InitWebClient().GET(OpenAi_ModelUrl);
            if(response.Success)
            {
                return Models.FromJSON(response.Text);
            }
            else throw new ChatGPTException($"{nameof(GetModels)}() failed - {response.Exception.Message}", response.Exception);
        }

        public CompletionResponse CompletionCreate(GPTPrompt p)
        {
            var response = InitWebClient().POST(p.Url, p.GetPostBody());
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = CompletionResponse.FromJson(response.Text);
                return r;
            }
            else throw new ChatGPTException($"{nameof(Translate)}() failed - {response.Exception.Message}", response.Exception);
        }

        // https://platform.openai.com/docs/guides/gpt
        public CompletionResponse ChatCompletionCreate(GPTPrompt p)
        {
            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(p.Url, p.GetPostBody());
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = CompletionResponse.FromJson(response.Text);
                r.GPTPrompt = p;
                r.Sw = sw;
                return r;
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
                Temperature = 0.2,
            };

            return CompletionCreate(prompt).Text.Trim();
        }

        public enum GPT_YesNoResponse
        {
            Yes,
            No,
            Unknown
        }

        public GPT_YesNoResponse IsThis(string systemContent, string yesNoQuestion, string dataText, string forceAnswerToYesNo = ", answer only with yes or no?")
        {
            var prompt = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = systemContent },
                    new GPTMessage{ Role =  MessageRole.user, Content = $"{yesNoQuestion}{forceAnswerToYesNo}{Environment.NewLine}{dataText}" }
                },
                Url = "https://api.openai.com/v1/chat/completions"
            };
            var response = this.ChatCompletionCreate(prompt);
            if (response.Success)
            {
                if(response.Text.ToLowerInvariant().Contains(GPT_YesNoResponse.Yes.ToString().ToLower()))
                    return GPT_YesNoResponse.Yes;
                if (response.Text.ToLowerInvariant().Contains(GPT_YesNoResponse.No.ToString().ToLower()))
                    return GPT_YesNoResponse.No;
                return GPT_YesNoResponse.Unknown;
            }
               
            else 
                throw new ChatGPTException($"{nameof(IsThis)}() failed - {response.ErrorMessage}");
        }

        public string Translate(string text, TranslationLanguages sourceLangague, TranslationLanguages targetLanguage)
        {
            var prompt = new Prompt_GPT_35_TurboInstruct
            {
                Text = $"Translate the following {sourceLangague} text to {targetLanguage}: '{text}'",
            };

            return CompletionCreate(prompt).Text.Trim();
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

