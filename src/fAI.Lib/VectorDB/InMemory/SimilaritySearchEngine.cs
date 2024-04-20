using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fAI.VectorDB
{
    // https://en.wikipedia.org/wiki/Cosine_similarity
    // https://cookbook.openai.com/
    public static class SimilaritySearchEngine
    {
        public static List<float> ToVector(string text)
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

        public static List<EmbeddingRecord> SimilaritySearch(
            List<float> queryVector, 
            List<EmbeddingRecord> embeddingRecords, int topK = 3, double minimumScore = 0.75)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(queryVector);
            var r = new List<EmbeddingRecord>();
            foreach (var er in embeddingRecords)
            {
                var score = CalculateCosineSimilarity(queryVector, er.Embedding);
                if (score > minimumScore)
                {
                    er.Score = score;
                    r.Add(er);
                }
            }
            r.Sort((a, b) => b.Score.CompareTo(a.Score));
            var rr = r.Take(topK).ToList();
            return rr;
        }

        public static double CalculateCosineSimilarity(List<float> vecA, List<float> vecB)
        {
            var dotProduct = DotProduct(vecA.ToArray(), vecB.ToArray());
            var magnitudeOfA = Magnitude(vecA.ToArray());
            var magnitudeOfB = Magnitude(vecB.ToArray());

            return dotProduct / (magnitudeOfA * magnitudeOfB);
        }

        private static double DotProduct(float[] vecA, float[] vecB)
        {
            double dotProduct = 0;
            for (var i = 0; i < vecA.Length; i++)
                dotProduct += (vecA[i] * vecB[i]);

            return dotProduct;
        }
        
        private static double Magnitude(float[] vector)
        {
            return Math.Sqrt(DotProduct(vector, vector));
        }
    }
}
