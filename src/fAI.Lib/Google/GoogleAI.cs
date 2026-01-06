using DynamicSugar;
using Mistral.SDK.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace fAI
{
    public class GoogleAI : Logger
    {
        public GoogleAI(int timeOut = -1, string openAiKey = null, string openAiOrg = null)
        {
            HttpBase._key = Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY");

            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;

            if (openAiKey != null)
                HttpBase._key = openAiKey;

            if (openAiOrg != null)
                HttpBase. _openAiOrg = openAiOrg;
        }
        public GoogleAICompletions _completions = null;
        public GoogleAICompletions Completions => _completions ?? (_completions = new GoogleAICompletions());
    }

    public partial class GoogleAICompletions : HttpBase, IOpenAICompletion
    {
        private string _key;
        public GoogleAICompletions(int timeOut = -1, string ApiKey = null, string openAiOrg = null) : base(timeOut, ApiKey, openAiOrg)
        {
            _key = ApiKey;
        }

        public string GetUrl(string model, string ApiKey)
        {
            return $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={ApiKey}";
        }

        const string DEFAULT_MODEL = "gemini-3-flash-preview";

        public List<string> GetModels()
        {
            return DS.List(
            "gemini-2.0-flash-lite",
            "gemini-3-flash-preview",
            "gemini-3-pro-preview"
            );
        }

        public dynamic GetPromptForTextImprovement(string userPrompt, string systemPrompt, string model = DEFAULT_MODEL, 
            float temperature = 0.7f, int maxOutputTokens = 1024 * 64)
        {
            var requestBody = new
            {
                system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                contents = new[] { new { role = "user", parts = new[] { new { text = userPrompt } } } },
                generationConfig = new { temperature = 0.7, maxOutputTokens = 800 }
            };
            return requestBody;
        }

        public string TextImprovement(
           string text,
           string language,
           string systemPrompt = @"
Improve the [language] for the following phrases, in more polished and business-friendly way:
===================================
            ",
            string model = DEFAULT_MODEL
           )
        {
            var contextPreProcessed = systemPrompt.Template(new
            {
                language,
            }, "[", "]");


            var client = new GoogleAI();
            var p = GetPromptForTextImprovement(text, contextPreProcessed, model);

            var url = GetUrl(model, _key);

            OpenAI.Trace(new { p.Url }, this);
            OpenAI.Trace(new { Prompt = p }, this);
            var body = JsonConvert.SerializeObject(p);
            OpenAI.Trace(new { Body = body }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(url, body);
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
                throw OpenAI.Trace(new ChatGPTException($"{response.Exception.Message}. {response.Text}", response.Exception));
            }
        }

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
                return new CompletionResponse { Exception = OpenAI.Trace(new ChatGPTException($"{response.Exception.Message}. {response.Text}", response.Exception)) };
            }
        }
    }
}
