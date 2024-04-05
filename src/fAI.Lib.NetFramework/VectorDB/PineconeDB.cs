using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Pinecone;
using Pinecone.Grpc;

namespace fAI.VectorDB
{
    public class PineconeDB
    {
        PineconeClient _pineconeClient;
        HttpClient _httpClient;

        public PineconeDB(int timeOut = -1, string key = null)
        {
            _httpClient = new HttpClient();
            _pineconeClient = new PineconeClient(GetKey(), GetEnvironment(), _httpClient);
        }

        public static string GetEnvironment()
        {
            return Environment.GetEnvironmentVariable("PINECONE_ENVIRONMENT"); // "us-east4-gcp"
        }

        public static string GetKey()
        {
            return Environment.GetEnvironmentVariable("PINECONE_API_KEY");
        }

        public void SelectIndex(string indexName)
        {
            var index = _pineconeClient.GetIndex(indexName).GetAwaiter().GetResult();
        }

        public void AddVectors(string indexName, List<string> ids, List<float[]> vectors, List<Dictionary<string, object>> metadatas)
        {
           
        }
    }
}
