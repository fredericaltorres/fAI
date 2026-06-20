using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using static fAI.GoogleAICompletions;
using static fAI.GoogleAICompletions.GoogleAICompletionsResponse;

namespace fAI
{
    public class OpenRouterCompletions : HttpBase, IOpenAICompletion
    {
        private string _key;
        public OpenRouterCompletions(int timeOut = -1, string ApiKey = null) : base(timeOut, ApiKey)
        {
            _key = ApiKey;
        }

        protected override ModernWebClient InitWebClient(bool addJsonContentType = true, Dictionary<string, object> extraHeaders = null)
        {
            var mc = new ModernWebClient(_timeout);
            if (addJsonContentType)
                mc.AddHeader("Content-Type", "application/json")
                  .AddHeader("Accept", "application/json")
                    .AddHeader("Authorization", $"Bearer {_key}");

            //foreach (string h in mc.Headers)
            //{
            //    OpenAI.Trace($"{h}:{mc.Headers[h]}", this);
            //}

            return mc;
        }

        public string GetUrl(string model)
        {
            return $"https://openrouter.ai/api/v1/chat/completions";
        }

        const string DEFAULT_MODEL = "deepseek/deepseek-v4-pro";

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

        public GeminiResponse Create(GoogleAICompletionsBody.GeminiPrompt p, string url, string model, List<fAI.Google.GoogleTool> tools = null)
        {
            var sw = Stopwatch.StartNew();

            if (tools != null && tools.Count > 0)
            {
                p.Tools = tools;
            }
            var body = JsonConvert.SerializeObject(p);

            OpenAI.Trace(new { Url = url }, this);
            //OpenAI.Trace(new { Prompt = p }, this);
            OpenAI.Trace(new { Body = body }, this);

            var response = InitWebClient().POST(url, body);
            sw.Stop();
            OpenAI.Trace(new { responseTime = sw.ElapsedMilliseconds / 1000.0, model }, this);

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
            throw new System.NotImplementedException();
        }
    }
}
