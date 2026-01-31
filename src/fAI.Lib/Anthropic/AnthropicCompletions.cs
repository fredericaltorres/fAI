using DynamicSugar;
using fAI.AnthropicLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using static DynamicSugar.DS;
using static System.Net.Mime.MediaTypeNames;

namespace fAI
{
    public partial class AnthropicCompletions : HttpBase//, IOpenAICompletion
    {
        public AnthropicCompletions(int timeOut = -1, string openAiKey = null) : base(timeOut,  openAiKey)
        {
        }

        protected override ModernWebClient InitWebClient(bool addJsonContentType = true, Dictionary<string, object> extraHeaders = null)
        {
            var mc = new ModernWebClient(_timeout);
            mc.AddHeader("x-api-key", _key)
              .AddHeader("anthropic-version", Anthropic.AnthropicApiVersion);

            if (addJsonContentType)
                mc.AddHeader("Content-Type", "application/json")
                  .AddHeader("Accept", "application/json");
            return mc;
        }

        public AnthropicCompletionResponse Create(AnthropicPromptBase p)
        {
            OpenAI.Trace(new { p.Url }, this);
            OpenAI.Trace(new { Prompt = p }, this);
            var body = p.GetPostBody();
            OpenAI.Trace(new { BodyLength = body.Length, Body = body }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(p.Url, body);
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);
                var r = AnthropicCompletionResponse.FromJson(response.Text);
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                OpenAI.TraceError(response.Exception.Message, this);
                return new AnthropicCompletionResponse { Exception = new ChatGPTException($"{response.Exception.Message}", response.Exception) };
            }
        }
    }
}

