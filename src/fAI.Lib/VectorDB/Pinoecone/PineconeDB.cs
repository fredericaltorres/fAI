using Deepgram.Models;
using fAI.Pinecone.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading;

namespace fAI.VectorDB
{
    // https://docs.pinecone.io/guides/getting-started/quickstart
    // https://docs.pinecone.io/guides/data/get-an-index-endpoint
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
        private const string DESCRIBE_INDEXES_URL = "https://api.pinecone.io/indexes/";

        private string UPSET_URL(string host) => $"https://{host}/vectors/upsert";
        private string CHECK_INDEX_URL(string host) => $"https://{host}/describe_index_stats";
        private string QUEY_INDEX_URL(string host) => $"https://{host}/query";

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

        public bool ExistsIndex(string indexName)
        {
            return DescribeIndex(indexName) != null;
        }

        public PineconeIndex DescribeIndex(string indexName)
        {
            var mc = InitWebClient();
            var response = mc.GET(DESCRIBE_INDEXES_URL + indexName);
            if (response.Success)
            {
                OpenAI.Trace(new { response.Text }, this);
                var r = PineconeIndex.FromJson(response.Text);
                return r;
            }
            else
            {
                return null;
            }
        }

        public List<float> ToVector(string text)
        {
            var client = new OpenAI();
            var r = client.Embeddings.Create(text);
            if (r.Success)
            {
                return r.Data[0].Embedding;
            }
            else
                return null;
        }

        public SimilaritySearchPayLoad SimilaritySearch(PineconeIndex index, string query, int topK, bool includeValues = false, string @namespace = "ns1", float minimumScore = -1)
        {
            var client = new OpenAI();
            var ebs = new List<EmbeddingRecord>();
            var queryVector = ToVector(query);
            if (queryVector != null)
            {
                var rr = SimilaritySearch(index, queryVector, topK, includeValues, @namespace);
                if (minimumScore != -1)
                    rr.matches = rr.matches.FindAll(m => m.score >= minimumScore);
                return rr;
            }
            else return new SimilaritySearchPayLoad { Exception = new PineconeException("Failed to get embedding") };
        }

        public SimilaritySearchPayLoad SimilaritySearch(PineconeIndex index, List<float> vector, int topK, bool includeValues = true, string @namespace = "ns1")
        {
            var sw = Stopwatch.StartNew();
            var mc = InitWebClient();
            var payload = new {
                @namespace,
                vector,
                topK,
                includeValues
            };
            var response = mc.POST(QUEY_INDEX_URL(index.host), JsonConvert.SerializeObject(payload));
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);
                var r = SimilaritySearchPayLoad.FromJson(response.Text);
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                return new SimilaritySearchPayLoad { Exception = new PineconeException($"{response.Exception.Message}", response.Exception) };
            }
        }

        public PineconeIndex GetIndex(string indexName)
        {
            return this.CheckIndex(this.DescribeIndex(indexName));
        }

        public PineconeIndex CheckIndex(PineconeIndex index)
        {
            var sw = Stopwatch.StartNew();
            var mc = InitWebClient();
            var response = mc.GET(CHECK_INDEX_URL(index.host));
            sw.Stop();
            if (response.Success)
            {
                OpenAI.Trace(new { response.Text }, this);
                var r = CheckIndexPayload.FromJson(response.Text);
                index.CheckIndexPayload = r;
                return index;
            }
            else return null;
        }

        public bool DeleteIndex(string indexName)
        {
            var mc = InitWebClient();
            var response = mc.DELETE(INDEXES_URL + "/" + indexName);
            if (response.Success)
            {
                OpenAI.Trace(new { response.Text }, this);
                return true;
            }
            else return false;
        }

        public PineconeIndex CreateIndex(
            string indexName,
            int dimension = 1536,
            string metric = "cosine",
            CloudNames cloud = CloudNames.aws,
            string region = "us-east-1",
            string environment = "us-east4-gcp"
        )
        {
            var sw = Stopwatch.StartNew();
            var mc = InitWebClient();
            var p = new CreateIndexInputPayload
            {
                name = indexName,
                dimension = dimension,
                metric = metric,
                spec = new Spec { serverless = new Serverless { cloud = cloud.ToString(), region = region } }
            };
            var response = mc.POST(INDEXES_URL, JsonConvert.SerializeObject(p));
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);
                var r = AnthropicCompletionResponse.FromJson(response.Text);
                r.Stopwatch = sw;
                WaitForConsistency();
                return CheckIndex(DescribeIndex(indexName));
            }
            else return null;
        }

        public void WaitForConsistency()
        {
            Thread.Sleep(1000 * 4); // Pinecone is eventually consistent
        }

        public UpsetResponse UpsertVectors(
            PineconeIndex index,
            List<PineconeVector> pineconeVectors,
            string nameSpace = "ns1")
        {
            var vectorContainer = new PineconeVectorContainer { @namespace = nameSpace };
            vectorContainer.vectors = pineconeVectors;
            
            var mc = InitWebClient();
            var url = UPSET_URL(index.host);
            var response = mc.POST(url, JsonConvert.SerializeObject(vectorContainer));
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);
                var r = UpsetResponse.FromJson(response.Text);
                return r;
            }
            else return new UpsetResponse { Exception = new PineconeException($"{response.Exception.Message}", response.Exception) };
        }
    }
}
