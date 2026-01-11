using DynamicSugar;
using Mistral.SDK.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static fAI.GoogleAICompletions.GoogleAICompletionsResponse;

namespace fAI
{
    public class GoogleAI : HttpBase
    {

        public static List<string> GetModels()
        {
            return DS.List(
"gemini-3-pro-preview",
"gemini-3-flash-preview",
"gemini-2.5-pro",
"gemini-2.5-flash",
"gemini-2.0-flash"
            );
        }

        public GoogleAI(int timeOut = -1, string ApiKey = null) 
        {
            base._key = Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY");

            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;

            if (ApiKey != null)
                base._key = ApiKey;

            //if (openAiOrg != null)
            //    HttpBase. _openAiOrg = openAiOrg;
        }
        public GoogleAICompletions _completions = null;
        public GoogleAICompletions Completions => _completions ?? (_completions = new GoogleAICompletions(ApiKey: base._key));
    }

    public partial class GoogleAICompletions : HttpBase, IOpenAICompletion
    {
        protected override ModernWebClient InitWebClient(bool addJsonContentType = true, Dictionary<string, object> extraHeaders = null)
        {
            var mc = new ModernWebClient(_timeout);
            if (addJsonContentType)
                mc.AddHeader("Content-Type", "application/json")
                  .AddHeader("Accept", "application/json")
                    .AddHeader("x-goog-api-key", _key);
            return mc;
        }

        public class GoogleAICompletionsResponse
        {
            public class Candidate
            {
                public Content content { get; set; }
                public string finishReason { get; set; }
                public int index { get; set; }
            }

            public class Content
            {
                public List<Part> parts { get; set; }
                public string role { get; set; }
            }

            public class Part
            {
                public string text { get; set; }
                public string thoughtSignature { get; set; }
            }

            public class PromptTokensDetail
            {
                public string modality { get; set; }
                public int tokenCount { get; set; }
            }

            public class GeminiResponse
            {
                public List<Candidate> candidates { get; set; }
                public UsageMetadata usageMetadata { get; set; }
                public string modelVersion { get; set; }
                public string responseId { get; set; }

                public static GeminiResponse FromJson(string text)
                {
                    return JsonUtils.FromJSON<GeminiResponse>(text);
                }

                public string GetText()
                {
                    if (candidates != null && candidates.Count > 0)
                    {
                        var parts = candidates[0].content.parts;
                        if (parts != null && parts.Count > 0)
                        {
                            return string.Join("", parts.Select(p => p.text));
                        }
                    }
                    return null;
                }
            }

            public class UsageMetadata
            {
                public int promptTokenCount { get; set; }
                public int candidatesTokenCount { get; set; }
                public int totalTokenCount { get; set; }
                public List<PromptTokensDetail> promptTokensDetails { get; set; }
                public int thoughtsTokenCount { get; set; }
            }
        }

        public class GoogleAICompletionsBody
        {
            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            public class Content
            {
                public string role { get; set; }
                public List<Part> parts { get; set; }
            }

            public class Part
            {
                public string text { get; set; }
            }

            public class GeminiPrompt
            {
                public SystemInstruction system_instruction { get; set; }
                public List<Content> contents { get; set; }

                public override string ToString()
                {
                    return ToJson();
                }
                public string ToJson()
                {
                    return JsonConvert.SerializeObject(this);
                }
            }

            public class SystemInstruction
            {
                public List<Part> parts { get; set; }
            }

        }

        private string _key;
        public GoogleAICompletions(int timeOut = -1, string ApiKey = null) : base(timeOut, ApiKey)
        {
            _key = ApiKey;
        }

        public string GetUrl(string model)
        {
            return $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";
            //return $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={ApiKey}";

        }

        const string DEFAULT_MODEL = "gemini-3-flash-preview";

        public GoogleAICompletionsBody.GeminiPrompt GetPrompt(string userPrompt, string systemPrompt, string model = DEFAULT_MODEL, 
            float temperature = 0.7f, int maxOutputTokens = 1024 * 64)
        {
            return new GoogleAICompletionsBody.GeminiPrompt
            {
                system_instruction = new GoogleAICompletionsBody.SystemInstruction
                {
                    parts = new List<GoogleAICompletionsBody.Part> { new GoogleAICompletionsBody.Part { text = systemPrompt } } 
                },
                contents = new List<GoogleAICompletionsBody.Content>
                {
                    new GoogleAICompletionsBody.Content
                    {
                        role = "user",
                        parts = new List<GoogleAICompletionsBody.Part> { new GoogleAICompletionsBody.Part { text = userPrompt } }
                    }
                }
            };
        }

        // curl.exe "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent?key=" -H "Content-Type: application/json" -d "{\"system_instruction\": {\"parts\": [{\"text\": \"You are a helpful assistant that speaks like a pirate.\"}]}, \"contents\": [{\"role\": \"user\", \"parts\": [{\"text\": \"Explain the concept of gravity.\"}]}]}"
        // curl.exe -X POST "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-pro-preview:generateContent?key=" -H "Content-Type: application/json" -d "{\"system_instruction\": {\"parts\": [{\"text\": \"You are a helpful assistant\"}]}, \"contents\": [{\"role\": \"user\", \"parts\": [{\"text\": \"Explain the concept of gravity.\"}]}]}"

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
            var contextPreProcessed = systemPrompt.Template(new { language }, "[", "]");

            var p = GetPrompt(text, contextPreProcessed, model);
            var url = GetUrl(model);
            var r = Create(p, url, model);
            return r.GetText();
        }

        public GeminiResponse Create(GoogleAICompletionsBody.GeminiPrompt p, string url, string model)
        {
            var sw = Stopwatch.StartNew();
            var body = JsonConvert.SerializeObject(p);

            OpenAI.Trace(new { Url = url }, this);
            OpenAI.Trace(new { Prompt = p }, this);
            OpenAI.Trace(new { Body = body }, this);

            var response = InitWebClient().POST(url, body);
            sw.Stop();
            OpenAI.Trace(new { responseTime = sw.ElapsedMilliseconds / 1000.0, model}, this);

            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);
                var geminiResponse = GeminiResponse.FromJson(response.Text);
                return geminiResponse;

            }
            else
            {
                throw OpenAI.Trace(new ChatGPTException($"{response.Exception.Message}. {response.Text}", response.Exception));
            }
        }

        public CompletionResponse Create(GPTPrompt p)
        {
            throw new NotImplementedException();
        }
    }
}
