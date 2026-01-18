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

    public partial class GroqCompletions : HttpBase, IOpenAICompletion
    {
        public GroqCompletions(int timeOut = -1, string openAiKey = null) : base(timeOut, openAiKey)
        {
        }

        //const string __url = "https://api.openai.com/v1/models";

        //public Models GetModels()
        //{
        //    var response = InitWebClient().GET(__url);
        //    if(response.Success)
        //    {
        //        return Models.FromJSON(response.Text);
        //    }
        //    else throw new ChatGPTException($"{nameof(GetModels)}() failed - {response.Exception.Message}", response.Exception);
        //}

        // https://platform.openai.com/docs/guides/gpt
        public AnthropicCompletionResponse Create(GPTPrompt p)
        {
            OpenAI.Trace(new { p.Url }, this);
            OpenAI.Trace(new { Prompt = p }, this);
            OpenAI.Trace(new { Body = p.GetPostBody() }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(p.Url, p.GetPostBody());
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);

                var r = AnthropicCompletionResponse.FromJson(response.Text);
                r.GPTPrompt = p;
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                return new AnthropicCompletionResponse { Exception = new ChatGPTException($"{response.Exception.Message}", response.Exception) };
            }
        }
    }
}

