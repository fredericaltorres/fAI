using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace fAI
{
    public class Datum
    {
        [JsonProperty(PropertyName = "object")]
        public string @Object { get; set; }

        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }

        [JsonProperty(PropertyName = "embedding")]
        public List<float> Embedding { get; set; }

        public string ToFloatList()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var f in Embedding)
                sb.Append(f.ToString() + "f, ");
            return sb.ToString();
        }

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
        public System.Exception Exception { get; set; }
        public bool Success => Exception == null;

        protected string DownloadImage(Uri uri)
        {
            string fileNameOnly = Path.GetFileName(uri.LocalPath);
            var fullPath = Path.Combine(Path.GetTempPath(), fileNameOnly);
            DownloadImageAsync(uri.ToString(), fullPath);
            return fullPath;
        }

        public void DownloadImageAsync(string imageUrl, string savePath)
        {
            if(File.Exists(savePath))
                File.Delete(savePath);

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(imageUrl).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    byte[] imageData = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    File.WriteAllBytes(savePath, imageData);
                }
                else
                {
                }
            }
        }
    }

    public class EmbeddingResponse : BaseHttpResponse
    {
        public string Text{ get; set; }

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

        public string GenerateCSharpCode(string variableName)
        {
            var vector = Data[0].ToFloatList();
            return $@"
const string {variableName}_TEXT = ""{Text}"";
List<float> {variableName}_VECTOR = new List<float>() {{ {vector} }};
";
        }
        
    }

   

}
