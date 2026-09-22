using fAI.OpenAI_Completions_Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static fAI.GenericAICompletions;

namespace fAI
{
    // doc https://openrouter.ai/docs/quickstart
    public partial class OpenRouterCompletions : HttpBase//, IOpenAICompletion
    {
        public OpenRouterCompletions(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }

        // https://openrouter.ai/deepseek/deepseek-v4-pro
        const string __urlLLM = "https://openrouter.ai/api/v1/chat/completions";
        const string __urlClassifier = "https://openrouter.ai/api/alpha/decisions";



        public AnthropicErrorCompletionResponse Create(GPTPromptEx p)
        {
            var url = __urlLLM;
            OpenAI.Trace(new { url }, this);
            //OpenAI.Trace(new { Prompt = p }, this);
            OpenAI.Trace(new { Body = p.GetPostBody() }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(url, p.GetPostBody());
            
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
                anthropicFormatResponse.Usage.ApiCost = openAIFormatResponse.usage.cost;

                //anthropicFormatResponse.GPTPrompt = p;
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

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Answers
        {
            public SafeToRun safe_to_run { get; set; }
        }

        public class ClassifierResponse
        {
            public string model { get; set; }
            public Answers answers { get; set; }
            public Usage usage { get; set; }
            public string id { get; set; }
            public string provider { get; set; }

            public static ClassifierResponse FromJson(string text)
            {
                return JsonUtils.FromJSON<ClassifierResponse>(text);
            }

            public Exception Exception { get; set; } = null;
            public bool Success => Exception == null;
            public Stopwatch Stopwatch { get; set; }    

            public bool Yes => answers?.safe_to_run?.Yes ?? false;
        }

        public class SafeToRun
        {
            public ClassifierType type { get; set; }
            public float noul { get; set; }

            public bool Yes => noul*100f > 50f;

            public Dictionary<string, string> probabilities { get; set; }

            public float confidence { get; set; }
            public string choice { get; set; }
        }

        public class Usage
        {
            public int input_tokens { get; set; }
            public int output_tokens { get; set; }
            public double cost { get; set; }
        }


        public (ClassifierResponse, GenericAIUsage) CreateClassifier(GenericAICompletions.ClassifierBody p)
        {
            var url = __urlClassifier;
            OpenAI.Trace(new { url }, this);
            OpenAI.Trace(new { Body = p.GetPostBody() }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(url, p.GetPostBody());

            sw.Stop();
            OpenAI.Trace(new { responseTime = sw.ElapsedMilliseconds / 1000.0, p.model }, this);
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);

                var classifierResponse = ClassifierResponse.FromJson(response.Text);

                var usage = new GenericAIUsage(p.model, "", "");
                usage.InputTokens = classifierResponse.usage.input_tokens;
                usage.OutputTokens = classifierResponse.usage.output_tokens;

                classifierResponse.Stopwatch = sw;
                return (classifierResponse, usage);
            }
            else
            {
                return (new ClassifierResponse { Exception = OpenAI.Trace(new ChatGPTException($"{response.Exception.Message}. {response.Text}", response.Exception)) }, new GenericAIUsage(p.model, "", ""));

            }
        }
    }
}

