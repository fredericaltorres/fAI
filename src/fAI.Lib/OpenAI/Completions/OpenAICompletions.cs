using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using static DynamicSugar.DS;
using DynamicSugar;

namespace fAI
{
    //public class FAICompletions : HttpBase, IOpenAICompletion
    //{
    //    public FAICompletions()
    //    {
    //    }

    //    public AnthropicErrorCompletionResponse Create(GPTPrompt p)
    //    {
    //        return new OpenAI().Completions.Create(p);
    //    }

    //    public AnthropicErrorCompletionResponse Create(AnthropicPromptBase p)
    //    {
    //        return new Anthropic().Completions.Create(p);
    //    }
    //}

    public partial class OpenAICompletions  : HttpBase, IOpenAICompletion
    {
        public OpenAICompletions(int timeOut = -1, string openAiKey = null) : base(timeOut, openAiKey)
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
        public AnthropicErrorCompletionResponse Create(GPTPrompt p)
        {
            OpenAI.Trace(new { p.Url }, this);
            OpenAI.Trace(new { Prompt = p }, this);
            OpenAI.Trace(new { Body = p.GetPostBody() }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(p.Url, p.GetPostBody());
            sw.Stop();
            OpenAI.Trace(new { responseTime = sw.ElapsedMilliseconds / 1000.0, p.Model }, this);
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);

                var r = AnthropicErrorCompletionResponse.FromJson(response.Text);
                r.GPTPrompt = p;
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                return new AnthropicErrorCompletionResponse { Exception = OpenAI.Trace(new ChatGPTException($"{response.Exception.Message}. {response.Text}", response.Exception)) };
            }
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
                //Url = GPTPrompt.OPENAI_URL_V1_CHAT_COMPLETIONS
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

        private bool IsValidJson<T>(string json)
        {
            try 
            {                 
                var o = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsNumeric(string value)
        {
            return value.All(char.IsNumber);
        }

        private bool IsNumeric(List<string> strings)
        {
            return strings.All(x => IsNumeric(x));
        }

    }
}

