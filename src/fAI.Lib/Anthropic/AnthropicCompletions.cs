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

    public partial class AnthropicCompletions : HttpBase, IOpenAICompletion
    {
        public AnthropicCompletions(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut,  openAiKey, openAiOrg)
        {
        }

        protected override ModernWebClient InitWebClient(bool addJsonContentType = true)
        {
            var mc = new ModernWebClient(_timeout);
            mc.AddHeader("x-api-key", _key)
              .AddHeader("anthropic-version", Anthropic.AnthropicApiVersion);

            if (addJsonContentType)
                mc.AddHeader("Content-Type", "application/json")
                  .AddHeader("Accept", "application/json");
            return mc;
        }

        // https://docs.anthropic.com/claude/reference/messages_post
        // https://docs.anthropic.com/claude/reference/messages-examples
        public CompletionResponse Create(GPTPrompt p)
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
    }
}

