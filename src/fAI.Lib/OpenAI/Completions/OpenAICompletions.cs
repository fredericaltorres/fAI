using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading;

namespace fAI
{
    public partial class OpenAICompletions  : OpenAIHttpBase
    {
        public OpenAICompletions(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut,  openAiKey, openAiOrg)
        {
        }

        const string __url = "https://api.openai.com/v1/models";

        public Models GetModels()
        {
            var response = InitWebClient().GET(__url);
            if(response.Success)
            {
                return Models.FromJSON(response.Text);
            }
            else throw new ChatGPTException($"{nameof(GetModels)}() failed - {response.Exception.Message}", response.Exception);
        }

        // https://platform.openai.com/docs/guides/gpt
        public CompletionResponse Create(GPTPrompt p)
        {
            OpenAI.Trace(new { prompt = p }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(p.Url, p.GetPostBody());
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = CompletionResponse.FromJson(response.Text);
                r.GPTPrompt = p;
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                return new CompletionResponse { Exception = new ChatGPTException($"{response.Exception.Message}", response.Exception) };
            }
        }

        public string Summarize(string text, TranslationLanguages sourceLangague) 
        {
            var prompt = new Prompt_GPT_35_TurboInstruct 
            {
                Text = text,
                PrePrompt = "Summarize the following text: \n===\n",
                PostPrompt = "\n===\nSummary:\n",
            };

            return Create(prompt).Text.Trim();
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
            var response = this.Create(prompt);
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

            return Create(prompt).Text.Trim();
        }

        public Dictionary<string, string> Translate(Dictionary<string, string> inputDictionary, TranslationLanguages sourceLangague, TranslationLanguages targetLanguage)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(inputDictionary, formatting: Formatting.Indented);
            var prompt = new Prompt_GPT_35_TurboInstruct
            {
                Text = $"Translate from {sourceLangague} to {targetLanguage} the following JSON blob:\r\n{json}",
            };
            var outputDictionaryJson = Create(prompt).Text.Trim();
            var od = JsonConvert.DeserializeObject<Dictionary<string, string>>(outputDictionaryJson);
            return od;
        }

        public List<string> Translate(List<string> strings, TranslationLanguages sourceLangague, TranslationLanguages targetLanguage)
        {
            var d = new Dictionary<string, string>();
            var intKey = 0;
            foreach (var s in strings)
            {
                d.Add(intKey.ToString(), s);
                intKey++;
            }
            var dd = Translate(d, sourceLangague, targetLanguage);
            return dd.Values.ToList();
        }
    }
}

