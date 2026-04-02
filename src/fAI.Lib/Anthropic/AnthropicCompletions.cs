using DynamicSugar;
using fAI.AnthropicLib;
using Mistral.SDK.Common;
using Mistral.SDK.DTOs;
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

        public AnthropicCompletionResponse CreateAgenticLoop( 
            string userPrompt,
            string systemPrompt = null,
            List<AnthropicTool> tools = null,
            FunctionCallers functionCallers = null)
        {
            var sw = Stopwatch.StartNew();
            var agenticLoopOn = true;
            var agenticLoopCounter = 0;
            var answer = "";

            var p = new Anthropic_Prompt_Claude_4_6_Sonnet()
            {
                Messages = new List<AnthropicMessage>()
                {
                    new AnthropicMessage { Role =  MessageRole.user, Content = DS.List<AnthropicContentMessage>(new AnthropicContentText(userPrompt)) }
                },
                Tools = new List<AnthropicTool>() { tools }
            };

            while (agenticLoopOn)
            {
                OpenAI.Trace($"[AGENTIC_LOOP] {DS.Dictionary(new { agenticLoopCounter, p.Model, sw.ElapsedMilliseconds }).Format()}", this);
                var response = new Anthropic().Completions.Create(p);
                if (!response.Success)
                {
                    throw new ApplicationException($"Request failed  {DS.Dictionary(new { response.StopReason }).Format()} ");
                }
                else if (response.Success && !response.HasFunctionCall)
                {
                    return response;
                }
                else if (response.Success && response.HasFunctionCall)
                {
                    var funcRequested = response.Content.GetFunctionCall();
                    var contentForNextCall = response.Content;
                    var fn = functionCallers[funcRequested.Name];
                    var param1Name = funcRequested.Input.Keys.ToList()[0];
                    var param1Value = funcRequested.Input[param1Name].ToString();
                    var funcData = fn.Call(param1Value);

                    p.Messages.Add(new AnthropicMessage { Role = MessageRole.assistant, Content = contentForNextCall });
                    p.Messages.Add(new AnthropicMessage
                    {
                        Role = MessageRole.user,
                        Content = DS.List<AnthropicContentMessage>(new AnthropicContentMessage()
                        {
                            Type = AnthropicContentMessageType.tool_result,
                            toolUseId = funcRequested.Id,
                            Content = DS.Dictionary(funcData).Format()
                        })
                    });
                }
            }

            return null;
        }

        public AnthropicCompletionResponse Create(AnthropicPromptBase p, Dictionary<string, string> extraHeaders = null)
        {
            OpenAI.Trace(new { p.Url }, this);
            OpenAI.Trace(new { Prompt = p }, this);
            var body = p.GetPostBody();
            OpenAI.Trace(new { BodyLength = body.Length, Body = body }, this);

            var sw = Stopwatch.StartNew();
            var wc = InitWebClient();

            if (extraHeaders != null)
            {
                foreach(var kv in extraHeaders)
                    wc.AddHeader(kv.Key, kv.Value);
            }

            var response = wc.POST(p.Url, body);
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

