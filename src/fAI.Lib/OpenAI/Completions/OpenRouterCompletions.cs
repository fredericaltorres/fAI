using fAI.OpenAI_Completions_Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace fAI
{
    public partial class OpenRouterCompletions : HttpBase, IOpenAICompletion
    {
        public OpenRouterCompletions(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }

        // https://openrouter.ai/deepseek/deepseek-v4-pro
        const string __url = "https://openrouter.ai/api/v1/chat/completions";

        public AnthropicErrorCompletionResponse Create(GPTPrompt p)
        {
            p.Url = __url;
            OpenAI.Trace(new { p.Url }, this);
            //OpenAI.Trace(new { Prompt = p }, this);
            OpenAI.Trace(new { Body = p.GetPostBody() }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(p.Url, p.GetPostBody());
            sw.Stop();
            OpenAI.Trace(new { responseTime = sw.ElapsedMilliseconds / 1000.0, p.Model }, this);
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);

                var openAIFormatResponse = OpenAICompletionResponse.FromJson(response.Text);

                var anthropicFormatResponse = AnthropicErrorCompletionResponse.FromJson(response.Text);
                anthropicFormatResponse.Usage = new AnthropicUsage();
                anthropicFormatResponse.Usage.InputTokens = openAIFormatResponse.usage.prompt_tokens;
                anthropicFormatResponse.Usage.OutputTokens = openAIFormatResponse.usage.completion_tokens;

                anthropicFormatResponse.GPTPrompt = p;
                anthropicFormatResponse.Stopwatch = sw;
                return anthropicFormatResponse;
            }
            else
            {
                return new AnthropicErrorCompletionResponse { Exception = OpenAI.Trace(new ChatGPTException($"{response.Exception.Message}. {response.Text}", response.Exception)) };
            }
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

