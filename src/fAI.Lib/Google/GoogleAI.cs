using DynamicSugar;
using Mistral.SDK.Common;
using Mistral.SDK.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static DynamicSugar.DS;
using static fAI.GenericAI;
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

        public GoogleAI(int timeOut = -1, string apiKey = null) 
        {
            base._key = Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY");

            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;

            if (apiKey != null)
                base._key = apiKey;
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
                public string finishMessage { get; set; }
            }

            public class Content
            {
                public List<Part> parts { get; set; }
                public string role { get; set; }

                [JsonIgnore]
                public bool HasFunctionCall => parts != null && parts.Any(p => p.IsFunctionCall);

                public FunctionCall GetFunctionCall()
                {
                    if (HasFunctionCall)
                        return parts.First(p => p.IsFunctionCall).functionCall;
                    return null;
                }
            }

            public class FunctionCall
            {
                public string name { get; set; }
                public string id { get; set; }
                public Dictionary<string, string> args { get; set; }
            }

            public class FunctionResponse
            {
                public string name { get; set; }
                public object response { get; set; }
            }

            public class Part
            {
                public string text { get; set; }
                public string thoughtSignature { get; set; }

                [JsonProperty("functionCall", NullValueHandling = NullValueHandling.Ignore)]
                public FunctionCall functionCall { get; set; }

                [JsonProperty("functionResponse", NullValueHandling = NullValueHandling.Ignore)]
                public FunctionResponse functionResponse { get; set; }

                [JsonIgnore]
                public bool IsFunctionCall => functionCall != null;

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

                [JsonIgnore]
                public bool HasFunctionCall => candidates != null && candidates.Count > 0 && candidates[0].content.HasFunctionCall;

                [JsonIgnore]
                public string FinishReason => candidates != null && candidates.Count > 0 ? candidates[0].finishReason : null;

                [JsonIgnore]
                public bool Success => FinishReason == "STOP";

                public static GeminiResponse FromJson(string text)
                {
                    return JsonUtils.FromJSON<GeminiResponse>(text);
                }

                [JsonIgnore]
                public string Text => GetText();

                [JsonIgnore]
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
            //public class Content
            //{
            //    public string role { get; set; }
            //    public List<Part> parts { get; set; }
            //}

            public class Part
            {
                public string text { get; set; }
            }

            public class GeminiPrompt
            {
                public SystemInstruction system_instruction { get; set; }
                public List<Content> contents { get; set; }

                [JsonProperty("tools", NullValueHandling = NullValueHandling.Ignore)]
                public List<fAI.Google.GoogleTool> Tools { get; set; } = null;

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

        public GoogleAICompletionsBody.GeminiPrompt GetPrompt(string userPrompt, string systemPrompt, string model = DEFAULT_MODEL, List<Content> contents = null)
        {
            var r = new GoogleAICompletionsBody.GeminiPrompt();

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                r.system_instruction = new GoogleAICompletionsBody.SystemInstruction
                {
                    parts = new List<GoogleAICompletionsBody.Part> { new GoogleAICompletionsBody.Part { text = systemPrompt } }
                };
            }
            if (contents == null)
            {
                r.contents = new List<Content>();
                r.contents.Add(new Content
                {
                    role = "user",
                    parts = new List<Part> { new Part { text = userPrompt } }
                });
            }
            else
            {
                r.contents = contents; // Expect the passed content to already contain the user prompt.
            }

            return r;
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

        public GeminiResponse CreateAgenticLoop(string userPrompt, string model, 
            string systemPrompt = null,
            List<fAI.Google.GoogleTool> tools = null,
            FunctionCallers functionCallers = null)
        {
            var sw = Stopwatch.StartNew();
            var r = new GeminiResponse();
            var agenticLoopOn = true;
            var agenticLoopCounter = 0;
            var answer = "";
            var url = this.GetUrl(model);
            var prompt = this.GetPrompt(userPrompt, systemPrompt, model);

            while (agenticLoopOn)
            {
                OpenAI.Trace($"[AGENTIC_LOOP] {DS.Dictionary(new { agenticLoopCounter, model, sw.ElapsedMilliseconds }).Format()}", this);

                // CALL STEP 1
                var rr = this.Create(prompt, url, model, tools: tools);
                if (!rr.Success)
                {
                    throw new ApplicationException($"Request failed  {DS.Dictionary(new { rr.FinishReason }).Format()} ");
                }
                else if (rr.Success && !rr.HasFunctionCall)
                {
                    answer = rr.GetText();
                    agenticLoopOn = false;
                    r = rr;
                    break;
                }
                else if (rr.Success && rr.HasFunctionCall)
                {
                    var funcRequested = rr.candidates.First().content.GetFunctionCall();
                    if (functionCallers.ContainsKey(funcRequested.name))
                    {
                        var fn = functionCallers[funcRequested.name];
                        var param1Name = fn.Arguments.Keys.ToList()[0];
                        var param1Value = funcRequested.args.Get(param1Name, "");
                        var funcData = fn.Call(param1Value); // CALL STEP 2 , Call the function with the arguments provided by LLM

                        // CALL STEP 4 , Call LLN with function result and all conversation history to get final answer
                        prompt.contents.Add(rr.candidates.First().content);
                        prompt.contents.Add(new GoogleAICompletions.GoogleAICompletionsResponse.Content
                        {
                            role = "function",
                            parts = new List<GoogleAICompletions.GoogleAICompletionsResponse.Part>()
                            {
                                new GoogleAICompletions.GoogleAICompletionsResponse.Part()
                                {
                                    functionResponse = new GoogleAICompletions.GoogleAICompletionsResponse.FunctionResponse()
                                    {
                                        name = funcRequested.name,
                                        response = funcData
                                    }
                                }
                            }
                        });
                    }
                }

                agenticLoopCounter += 1;
            } // agenticLoopOn

            sw.Stop();
            OpenAI.Trace($"[AGENTIC_LOOP][DONE] {DS.Dictionary(new { model, sw.ElapsedMilliseconds }).Format()}", this);

            return r;
        }

        public GeminiResponse Create(GoogleAICompletionsBody.GeminiPrompt p, string url, string model, List<fAI.Google.GoogleTool> tools = null)
        {
            var sw = Stopwatch.StartNew();

            if(tools != null && tools.Count > 0)
            {
                p.Tools = tools;
            }
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

        public AnthropicErrorCompletionResponse Create(GPTPrompt p)
        {
            throw new NotImplementedException();
        }
    }
}
