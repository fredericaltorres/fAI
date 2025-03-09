using Mistral.SDK.DTOs;
using Mistral.SDK;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;


namespace fAI
{
    // Add the attribute `[JsonProperty("")]` to the properties of the class to match the JSON keys. and rename the properties in the class according to the C SHARP standards.

    public class MistralCapabilities
    {
        [JsonProperty("completion_chat")]
        public bool CompletionChat { get; set; }

        [JsonProperty("completion_fim")]
        public bool CompletionFim { get; set; }

        [JsonProperty("function_calling")]
        public bool FunctionCalling { get; set; }

        [JsonProperty("fine_tuning")]
        public bool FineTuning { get; set; }

        [JsonProperty("vision")]
        public bool Vision { get; set; }
    }

    public class MistralDatum
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("created")]
        public int Created { get; set; }

        [JsonProperty("owned_by")]
        public string OwnedBy { get; set; }

        [JsonProperty("capabilities")]
        public MistralCapabilities Capabilities { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("max_context_length")]
        public int MaxContextLength { get; set; }

        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; }

        [JsonProperty("deprecation")]
        public object Deprecation { get; set; }

        [JsonProperty("default_model_temperature")]
        public double? DefaultModelTemperature { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class mistralMistralModel
    {
        public string @object { get; set; }
        public List<MistralDatum> data { get; set; }

        public static mistralMistralModel FromJson(string json) => 
            JsonConvert.DeserializeObject<mistralMistralModel>(json);
    }
}

namespace fAI
{
    public partial class MistralCompletions : HttpBase//, IOpenAICompletion
    {
        public MistralCompletions(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut, openAiKey, openAiOrg)
        {
        }

        public List<MistralDatum> GetModels()
        {
            var url = "https://api.mistral.ai/v1/models";

            OpenAI.Trace(new { url }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().GET(url);
            sw.Stop();
            if (response.Success)
            {
                OpenAI.Trace(new { response.Text }, this);
                return mistralMistralModel.FromJson(response.Text).data;
            }
            else
            {
                OpenAI.TraceError(response.Exception.Message, this);
                throw response.Exception;
            }
        }

        protected override ModernWebClient InitWebClient(bool addJsonContentType = true)
        {
            var mc = new ModernWebClient(_timeout);
            mc.AddHeader("Authorization", $"Bearer {_key}");

            if (addJsonContentType)
                mc.AddHeader("Content-Type", "application/json")
                  .AddHeader("Accept", "application/json");
            return mc;
        }

        public CompletionResponse Create(MistralPromptBase p)
        {
            OpenAI.Trace(new { p.Url }, this);
            OpenAI.Trace(new { Prompt = p }, this);
            var body = p.GetPostBody();
            OpenAI.Trace(new { BodyLenght = body.Length, Body = body }, this);

            var sw = Stopwatch.StartNew();
            var client = new MistralClient();

            var request = new ChatCompletionRequest(
                ModelDefinitions.MistralMedium,
                p.Messages
                // Optional parameters
                //safePrompt: p.SafePrompt,
                //temperature: (decimal)p.Temperature,
                //maxTokens: p.MaxTokens,
                //topP: p.TopP,
                //randomSeed: p.RandomSeed 
            );

            if (p._responseFormatAsJson)
            {
                request.ResponseFormat = new global::Mistral.SDK.DTOs.ResponseFormat()
                {
                    Type = global::Mistral.SDK.DTOs.ResponseFormat.ResponseFormatEnum.JSON
                };
            }

            // Get the completion response
            var response = client.Completions.GetCompletionAsync(request).GetAwaiter().GetResult();

            sw.Stop();
            if (!string.IsNullOrEmpty(response.Id))
            {
                OpenAI.Trace(new { response.Choices[0].Message.Content }, this);
                var r = new CompletionResponse();
                r.Choices = new List<CompletionChoiceResponse>();
                r.Choices.Add(new CompletionChoiceResponse { 
                    message = new GPTMessage { 
                        Content = response.Choices[0].Message.Content,
                        //Role = response.Choices[0].
                    }
                });
                r.Choices[0].finish_reason = CompletionResponse.FULL_SUCCEES_RETURN_CODE;
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                //OpenAI.TraceError(response.Exception.Message, this);
                //return new CompletionResponse { Exception = new ChatGPTException($"{response.Exception.Message}", response.Exception) };
                return null;
            }
        }
    }
}

