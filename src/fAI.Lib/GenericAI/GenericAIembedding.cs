using DynamicSugar;
using fAI.GenericAIembeddings;
using fAI.Util.Strings;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static fAI.GenericAIUtility;
using static System.Net.WebRequestMethods;

namespace fAI
{
    namespace GenericAIembeddings
    {
        public class Architecture
        {
            public List<string> input_modalities { get; set; }
            public object instruct_type { get; set; }
            public string modality { get; set; }
            public List<string> output_modalities { get; set; }
            public string tokenizer { get; set; }
        }

        public class DatumEmbeddingModel
        {
            public Architecture architecture { get; set; }
            public string canonical_slug { get; set; }
            public int context_length { get; set; }
            public int created { get; set; }
            public object default_parameters { get; set; }
            public string description { get; set; }
            public object expiration_date { get; set; }
            public string id { get; set; }
            public object knowledge_cutoff { get; set; }
            public Links links { get; set; }
            public string name { get; set; }
            public object per_request_limits { get; set; }
            public Pricing pricing { get; set; }
            public List<object> supported_parameters { get; set; }
            public object supported_voices { get; set; }
            public TopProvider top_provider { get; set; }
        }

        public class Links
        {
            public string details { get; set; }
        }

        public class Pricing
        {
            public string completion { get; set; }
            public string image { get; set; }
            public string prompt { get; set; }
            public string request { get; set; }
        }

        public class GetEmbeddingModelsApi
        {
            public List<DatumEmbeddingModel> data { get; set; }
            public static GetEmbeddingModelsApi FromJson(string json) => JsonConvert.DeserializeObject<GetEmbeddingModelsApi>(json);
        }

        public class TopProvider
        {
            public int context_length { get; set; }
            public bool is_moderated { get; set; }
            public object max_completion_tokens { get; set; }
        }
    }

    public class GenericAIembedding : HttpBase
    {
        //https://openrouter.ai/docs/api/api-reference/images/generate-an-image
        public const string __url = "https://openrouter.ai/api/v1/embeddings";

        public GenericAIembedding(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }


        public class GenericAIembeddingModels
        {
            public string Id { get; set; }
            public int Dimensions { get; set; }

            public GenericAIembeddingModels(DatumEmbeddingModel d)
            {
                Id = d.id;
                Dimensions = d.context_length;
            }
        }

        public List<GenericAIembeddingModels> GetModelsApi()
        {
            OpenAI.Trace(new { }, this);

            if (base._key == null)
                base._key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

            var wc = InitWebClient();
            var response = wc.GET("https://openrouter.ai/api/v1/embeddings/models?limit=500");
            if (response.Success)
            {
                var r = GetEmbeddingModelsApi.FromJson(response.Text);
                Logger.Trace(response.Text, this);
                return r.data.Select(x => new GenericAIembeddingModels(x)).ToList();
            }
            else throw new OpenAIAudioSpeechException($"{nameof(GetModelsApi)}() failed - {response.Exception.Message}", response.Exception);
        }

        public class Datum
        {
            public List<float> embedding { get; set; }
            public int index { get; set; }
            public string @object { get; set; }
        }

        public class EmbeddingResponse
        {
            public List<Datum> data { get; set; }
            public string model { get; set; }
            public string @object { get; set; }
            public Usage usage { get; set; }

            public static EmbeddingResponse FromJson(string json) => JsonConvert.DeserializeObject<EmbeddingResponse>(json);
        }

        public class Usage
        {
            public int prompt_tokens { get; set; }
            public int total_tokens { get; set; }
        }

        public (List<float>, GenericAICompletions.GenericAIUsage usage) Create(
            string text,
            string model = "openai/text-embedding-3-small",
            int dimension = 1536,
            string filePath = null
            )
        {
            OpenAI.Trace(new { model, text}, this);
            var sw = Stopwatch.StartNew();
            var usage = new GenericAICompletions.GenericAIUsage(model, "","");
            if (base._key == null)
                base._key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
            var wc = InitWebClient();
            var response = wc.POST(__url, GetPayLoad(text, model, dimension));
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = EmbeddingResponse.FromJson(response.Text);
                sw.Stop();
                usage.InputTokens = r.usage.prompt_tokens;
                usage.OutputTokens = 0;
                usage.SetDuration(sw);

                OpenAI.Trace($"[EMBEDDING] Duration: {sw.ElapsedMilliseconds:00000} ms, Model: {model}", this);

                return (r.data.First().embedding, usage);
            }
            else throw new OpenAIAudioSpeechException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
        }

        private string GetPayLoad(string input, string model, int dimensions)
        {
            if (dimensions == 1536)
            {
                return JsonConvert.SerializeObject(new
                {
                    input,
                    model,
                    dimensions
                });
            }
            else
            {
                return JsonConvert.SerializeObject(new
                {
                    input,
                    model
                });
            }
        }
    }
}



