using Deepgram.Models;
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
        List<EmbeddingRecord> embeddingRecords,
        int topK = 3,
        double minimumScore = 0.75)
        {
            if (queryVector == null || queryVector.Count == 0)
                throw new ArgumentException("Query vector cannot be null or empty.");

            if (embeddingRecords == null || embeddingRecords.Count == 0)
                return new List<EmbeddingRecord>();

            var scored = new List<EmbeddingRecord>();
            double maxScore = double.MinValue;

            foreach (var record in embeddingRecords)
            {
                if (record.Embedding == null || record.Embedding.Count != queryVector.Count)
                    continue; // skip invalid vectors

                var score = CosineSimilarity(queryVector, record.Embedding);
                record.Score = score;
                maxScore = Math.Max(maxScore, score);

                if (score >= minimumScore || score == -1f)
                {
                    scored.Add(record);
                }
            }

            return scored
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .Select(x => x)
                .ToList();
        }

        private static double CosineSimilarity(List<float> v1, List<float> v2)
        {
            double dot = 0.0;
            double norm1 = 0.0;
            double norm2 = 0.0;

            for (int i = 0; i < v1.Count; i++)
            {
                dot += v1[i] * v2[i];
                norm1 += v1[i] * v1[i];
                norm2 += v2[i] * v2[i];
            }

            if (norm1 == 0 || norm2 == 0)
                return 0;

            return dot / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }



        public static List<EmbeddingRecord> SimilaritySearch_old(
            List<float> queryVector, 
            List<EmbeddingRecord> embeddingRecords, int topK = 3, double minimumScore = 0.75)
        {
            //var json = Newtonsoft.Json.JsonConvert.SerializeObject(queryVector);
            var r = new List<EmbeddingRecord>();
            foreach (var er in embeddingRecords)
            {
                var score = CalculateCosineSimilarity_old(queryVector, er.Embedding);
                if (score > minimumScore)
                {
                    er.Score = (float)Math.Round(score, 4);
                    r.Add(er);
                }
            }
            r.Sort((a, b) => b.Score.CompareTo(a.Score));
            var rr = r.Take(topK).ToList();
            return rr;
        }

        public static double CalculateCosineSimilarity_old(List<float> vecA, List<float> vecB)
        {
            var dotProduct = DotProduct_old(vecA.ToArray(), vecB.ToArray());
            var magnitudeOfA = Magnitude_old(vecA.ToArray());
            var magnitudeOfB = Magnitude_old(vecB.ToArray());

            return dotProduct / (magnitudeOfA * magnitudeOfB);
        }

        private static double DotProduct_old(float[] vecA, float[] vecB)
        {
            double dotProduct = 0;
            for (var i = 0; i < vecA.Length; i++)
                dotProduct += (vecA[i] * vecB[i]);

            return dotProduct;
        }
        
        private static double Magnitude_old(float[] vector)
        {
            return Math.Sqrt(DotProduct_old(vector, vector));
        }
    }
}
