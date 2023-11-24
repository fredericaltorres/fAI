using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace fAI
{
    public class Datum
    {
        [JsonProperty(PropertyName = "object")]
        public string @Object { get; set; }

        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }

        [JsonProperty(PropertyName = "embedding")]
        public List<double> Embedding { get; set; }

        public readonly int EmbeddingMaxValue = 1536;

    }

  
    public class Usage2
    {
        [JsonProperty(PropertyName = "prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty(PropertyName = "total_tokens")]
        public int TotalTokens { get; set; }
    }

    public class BaseHttpResponse
    {
        public Stopwatch Stopwatch { get; set; }
    }

    public class EmbeddingResponse : BaseHttpResponse
    {
        [JsonProperty(PropertyName = "object")]
        public string @Object { get; set; }

        [JsonProperty(PropertyName = "data")]
        public List<Datum> Data { get; set; }

        [JsonProperty(PropertyName = "model")]
        public string Model { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public Usage2 Usage { get; set; }

        public static EmbeddingResponse FromJson(string text)
        {
            return JsonUtils.FromJSON<EmbeddingResponse>(text);
        }
    }

   

}
