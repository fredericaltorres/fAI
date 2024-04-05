using System;
using System.Collections.Generic;
using System.Text;

namespace fAI.Pinecone.Model
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class PineconeIndex
    {
        public string name { get; set; }
        public string metric { get; set; }
        public int dimension { get; set; }
        public Status status { get; set; }
        public string host { get; set; }
        public Spec spec { get; set; }

        public static PineconeIndex FromJson(string json) => 
            Newtonsoft.Json.JsonConvert.DeserializeObject<PineconeIndex>(json);
    }

    public class Serverless
    {
        public string region { get; set; }
        public string cloud { get; set; }
    }

    public class Spec
    {
        public Serverless serverless { get; set; }
    }

    public class Status
    {
        public bool ready { get; set; }
        public string state { get; set; }
    }









    public class CreateIndexInputPayload
    {
        public string name { get; set; }
        public int dimension { get; set; }
        public string metric { get; set; }
        public Spec spec { get; set; }
    }

    //public class Serverless
    //{
    //    public string cloud { get; set; }
    //    public string region { get; set; }
    //}

    //public class Spec
    //{
    //    public Serverless serverless { get; set; }
    //}

    public enum CloudNames
    {
        aws = 1,
        GCP = 2
    }

    public class PineconeVector
    {
        public string Id { get; set; }
        public List<float> Values { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}
