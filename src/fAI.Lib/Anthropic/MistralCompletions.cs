using Mistral.SDK.DTOs;
using Mistral.SDK;
using System.Collections.Generic;
using System.Diagnostics;

namespace fAI
{
    public partial class MistralCompletions : HttpBase//, IOpenAICompletion
    {
        public MistralCompletions(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut, openAiKey, openAiOrg)
        {
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
                //response.Choices.
                //response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Choices[0].Message }, this);
                var r = new CompletionResponse();
                r.Choices = new List<CompletionChoiceResponse>();
                r.Choices.Add(new CompletionChoiceResponse { 
                    message = new GPTMessage { 
                        Content = response.Choices[0].Message.Content,
                        //Role = response.Choices[0].
                    }
                });

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

