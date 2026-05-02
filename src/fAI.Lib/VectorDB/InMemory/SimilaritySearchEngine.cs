using Deepgram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace fAI.VectorDB
{
    // https://en.wikipedia.org/wiki/Cosine_similarity
    // https://cookbook.openai.com/
    public static class SimilaritySearchEngine
    {
        public static float GetOpenAIEmbeddingDynamicScore(float bestScore)
        {
            var minimumScore = 0f;
            if (bestScore > 0.7f)
                minimumScore = bestScore * 0.50f;
            else if (bestScore > 0.6f)
                minimumScore = bestScore * 0.55f;
            else if (bestScore > 0.5f)
                minimumScore = bestScore * 0.70f;
            else if (bestScore > 0.4f)
                minimumScore = bestScore * 0.80f;
            else if (bestScore > 0.35f)
                minimumScore = bestScore * 0.85f;
            else if (bestScore > 0.3f)
                minimumScore = bestScore * 0.90f;
            else if (bestScore > 0.2f)
                minimumScore = bestScore * 0.95f;

            return minimumScore;
        }

        public static List<float> ToVector(string text, string apiKey = null)
        {
            var client = new OpenAI(apiKey: apiKey);
            var r = client.Embeddings.Create(text);
            if (r.Success)
            {
                return r.Data[0].Embedding;
            }
            else
                return null;
        }

        public static List<EmbeddingCommonRecord> SimilaritySearch(
            List<float> queryVector,
            List<EmbeddingCommonRecord> embeddingRecords,
            int topK = 3,
            double minimumScore = 0.75)
        {
            if (queryVector == null || queryVector.Count == 0)
                throw new ArgumentException("Query vector cannot be null or empty.");

            if (embeddingRecords == null || embeddingRecords.Count == 0)
                return new List<EmbeddingCommonRecord>();

            var scored = new List<EmbeddingCommonRecord>();
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

        internal static double CosineSimilarity(List<float> v1, List<float> v2)
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



        public static List<EmbeddingCommonRecord> SimilaritySearch_old(
            List<float> queryVector, 
            List<EmbeddingCommonRecord> embeddingRecords, int topK = 3, double minimumScore = 0.75)
        {
            //var json = Newtonsoft.Json.JsonConvert.SerializeObject(queryVector);
            var r = new List<EmbeddingCommonRecord>();
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
