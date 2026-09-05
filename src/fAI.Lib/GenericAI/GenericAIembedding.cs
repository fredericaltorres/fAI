using DynamicSugar;
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
    public class GenericAIembedding : HttpBase
    {
        //https://openrouter.ai/docs/api/api-reference/images/generate-an-image
        public const string __url = "https://openrouter.ai/api/v1/embeddings";

        public GenericAIembedding(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }

        public List<string> GetCheapModels()
        {
            return DS.List(
                "meta/muse-image"
                );
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
                usage.OutputTokens = r.usage.total_tokens;
                usage.SetDuration(sw);

                OpenAI.Trace($"[EMBEDDING] Duration: {sw.ElapsedMilliseconds:00000} ms, Model: {model}", this);

                return (r.data.First().embedding, usage);
            }
            else throw new OpenAIAudioSpeechException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
        }

        private string GetPayLoad(string input, string model, int dimensions)
        {
            return JsonConvert.SerializeObject(new
            {
                input,
                model,
                dimensions
            });
        }
    }
}



