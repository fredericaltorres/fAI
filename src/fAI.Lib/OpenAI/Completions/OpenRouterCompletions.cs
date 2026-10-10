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

        const string __urlLLM           = "https://openrouter.ai/api/v1/chat/completions";
        const string __urlClassifier    = "https://openrouter.ai/api/alpha/decisions";

        const string OLLMA_TAG = "ollama/";


        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class OllamaMessage
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        public class OllamaResponse
        {
            public string model { get; set; }
            public DateTime created_at { get; set; }
            public OllamaMessage message { get; set; }
            public bool done { get; set; }
            public string done_reason { get; set; }
            public long total_duration { get; set; }
            public int load_duration { get; set; }
            public int prompt_eval_count { get; set; }
            public int prompt_eval_cached_count { get; set; }
            public int prompt_eval_duration { get; set; }
            public int eval_count { get; set; }
            public long eval_duration { get; set; }

            public AnthropicUsage Usage { get; set; }
            public Stopwatch Stopwatch { get; set; }

            public static OllamaResponse FromJson(string text)
            {
                return JsonUtils.FromJSON<OllamaResponse>(text);
            }
        }

        public AnthropicErrorCompletionResponse CreateLocalOllamaLLM(GPTPromptEx p)
        {
            var url = "http://localhost:11434/api/chat";
            p.Model = p.Model.Replace(OLLMA_TAG, "");

            OpenAI.Trace(new { url }, this);
            var body = p.GetPostBodyOllama();
            OpenAI.Trace(new { BodyLenKb = (body.Length / 1024.0).ToString("0.0"), BodyWords = new OpenAIEmbeddings().CountWords(body), Body = body }, this);
            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(url, body);
            sw.Stop();

            OpenAI.Trace(new { responseTime = sw.ElapsedMilliseconds / 1000.0, p.Model }, this);
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);

                var r = OllamaResponse.FromJson(response.Text);

                var rr = new AnthropicErrorCompletionResponse();
                rr.Choices = new List<CompletionChoiceResponse>();
                rr.Choices.Add(new CompletionChoiceResponse());
                rr.Choices.First().message = new GPTMessage() {
                    Role = Enum.TryParse<MessageRole>(r.message.role, out var role) ? role : MessageRole.user,
                    Content = r.message.content,
                };
                rr.Stopwatch = sw;
                rr.Usage = r.Usage;
                rr.Usage = new AnthropicUsage();
                rr.Usage.InputTokens = 0;
                rr.Usage.OutputTokens = 0;
                rr.Usage.ApiCost = 0;

                //rr.Content = response.Text;
                return rr;
            }
            else
            {
                return new AnthropicErrorCompletionResponse { Exception = OpenAI.Trace(new ChatGPTException($"{response.Exception.Message}. {response.Text}", response.Exception)) };
            }
        }

        public AnthropicErrorCompletionResponse Create(GPTPromptEx p)
        {
            var url = __urlLLM;
            var OLLMA_TAG = "ollama/";
            if (p.Model.StartsWith(OLLMA_TAG))
            {
                return CreateLocalOllamaLLM(p);
            }

            OpenAI.Trace(new { url }, this);
            var body = p.GetPostBody();

            OpenAI.Trace(new { BodyLenKb = (body.Length / 1024.0).ToString("0.0"),  BodyWords = new OpenAIEmbeddings().CountWords(body), Body = body }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(url, body);
            sw.Stop();

            OpenAI.Trace(new { responseTime = sw.ElapsedMilliseconds / 1000.0, p.Model }, this);
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);

                var openAIFormatResponse = OpenAICompletionResponse.FromJson(response.Text);

                var r = AnthropicErrorCompletionResponse.FromJson(response.Text);
                r.Usage = new AnthropicUsage();
                r.Usage.InputTokens = openAIFormatResponse.usage.prompt_tokens;
                r.Usage.OutputTokens = openAIFormatResponse.usage.completion_tokens;
                r.Usage.ApiCost = openAIFormatResponse.usage.cost;
                r.Stopwatch = sw;
                return r;
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
        public class Answers : Dictionary<string, AnswerForQuestion>
        {
            //public AnswerForQuestion question { get; set; }
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

            public bool Yes => answers?.Values.FirstOrDefault()?.Yes ?? false;
        }

        public class AnswerForQuestion
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
            public float cost { get; set; }
        }


        public (ClassifierResponse, GenericAIUsage) CreateClassifier(GenericAICompletions.ClassifierBody p)
        {
            var url = __urlClassifier;
            OpenAI.Trace(new { url }, this);
            OpenAI.Trace(new { Body = p.GetPostBody() }, this);

            var wc = InitWebClient();
            var sw = Stopwatch.StartNew();
            var response = wc.POST(url, p.GetPostBody());
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
                usage.ApiCost = (float)classifierResponse.usage.cost;

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

