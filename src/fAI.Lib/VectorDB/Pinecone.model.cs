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

        public CheckIndexPayload CheckIndexPayload { get; set; }

        public int totalVectorCount => CheckIndexPayload == null ? 0 : CheckIndexPayload.totalVectorCount;

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
        public string id { get; set; }
        public List<float> values { get; set; }
        public Dictionary<string, object> metadata { get; set; }
    }

    public class PineconeVectorContainer
    {
        public List<PineconeVector> vectors { get; set; }
        public string @namespace { get; set; } = "ns1";
    }

    public class UpsetResponse
    {
        public int upsertedCount { get; set; }

        public static UpsetResponse FromJson(string json) =>
            Newtonsoft.Json.JsonConvert.DeserializeObject<UpsetResponse>(json);
    }


    public class Namespaces
    {
        public Ns1 ns1 { get; set; } // This should be dynamic and not typed
    }

    public class Ns1
    {
        public int vectorCount { get; set; }
    }

    public class CheckIndexPayload  
    {
        public Namespaces namespaces { get; set; }
        public int dimension { get; set; }
        public int indexFullness { get; set; }
        public int totalVectorCount { get; set; }

        public static CheckIndexPayload FromJson(string json) =>
            Newtonsoft.Json.JsonConvert.DeserializeObject<CheckIndexPayload>(json);
    }
}

