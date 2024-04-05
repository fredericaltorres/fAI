using Deepgram.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace fAI.VectorDB
{
    // https://docs.pinecone.io/guides/getting-started/quickstart
    public class PineconeDB : Logger
    {
        private string _key;
        private string _environment;
        public PineconeDB(int timeOut = -1, string key = null)
        {
            _key = Environment.GetEnvironmentVariable("PINECONE_API_KEY");
            _environment = Environment.GetEnvironmentVariable("PINECONE_ENVIRONMENT");
        }

        private const string INDEXES_URL = "https://api.pinecone.io/indexes";

        protected ModernWebClient InitWebClient(bool addJsonContentType = true)
        {
            var mc = new ModernWebClient(HttpBase._timeout);
            mc.AddHeader("Api-Key", _key)
              .AddHeader("OpenAI-Organization", _environment);

            if (addJsonContentType)
                mc.AddHeader("Content-Type", "application/json")
                  .AddHeader("Accept", "application/json");
            return mc;
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class CreateIndexInputPayload
        {
            public string name { get; set; }
            public int dimension { get; set; }
            public string metric { get; set; }
            public Spec spec { get; set; }
        }

        public class Serverless
        {
            public string cloud { get; set; }
            public string region { get; set; }
        }

        public class Spec
        {
            public Serverless serverless { get; set; }
        }

        public bool CreateIndex(string indexName, int dimension = 1536, string metric = "cosine", string cloud = "aws", string region = "us-east4-gcp")
        {
            var sw = Stopwatch.StartNew();
            var mc = InitWebClient();
            var p = new CreateIndexInputPayload 
            {
                name = indexName,
                dimension = dimension,
                metric = metric,
                spec = new Spec { serverless = new Serverless { cloud = cloud, region = region /*"us-west-2"*/ } }
            };
            var response = mc.POST(INDEXES_URL, JsonConvert.SerializeObject(p));
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);
                var r = CompletionResponse.FromJson(response.Text);
                r.Stopwatch = sw;
                return r.Success;
            }
            else
            {
                //return new CompletionResponse { Exception = new ChatGPTException($"{response.Exception.Message}", response.Exception) };
                return false;
            }
        }

        public void AddVectors(string indexName, List<string> ids, List<float[]> vectors, List<Dictionary<string, object>> metadatas)
        {
           
        }
    }
}
