using DynamicSugar;
using fAI.Pinecone.Model;
using fAI.VectorDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fAI.RevolverConsole
{
    internal class Program
    {
        const string BeatlesRevolverIndexName = "beetles-revolver";

        public static void CreateIndex()
        {
            var client = new PineconeDB();
            if (!client.ExistsIndex(BeatlesRevolverIndexName))
            {
                var index = client.CreateIndex(BeatlesRevolverIndexName);
                var e = EmbeddingRecord.FromJsonFile(@".\Revolver.json");

                foreach (var er in e)
                {
                    var pv = new PineconeVector { id = er.Id, values = er.Embedding, metadata = DS.Dictionary(new { text = er.Text }) };
                    var r1 = client.UpsertVectors(index, new List<PineconeVector> { pv });
                }
            }
        }

        static void Write(string message, ConsoleColor color)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = originalColor;
        }

        static void WriteLine(string message, ConsoleColor color)
        {
            Write(message, color);
            Console.WriteLine();
        }

        static void WriteQuestion(string message) => WriteLine(message, ConsoleColor.Yellow);
        static void WriteInformation(string message)  => WriteLine(message, ConsoleColor.White);
        static void WriteAnswer(string message) => WriteLine(message, ConsoleColor.Green);

        static void Main(string[] args)
        {
            Console.Clear();
            CreateIndex();
            var message = "Enter search criteria about the lyrics of the Beatles' Album Revolver ?";
            WriteQuestion(message);
            WriteInformation("Enter 'exit' to quit.");

            var client = new PineconeDB();
            var index = client.GetIndex(BeatlesRevolverIndexName);
            var minimumScore = 0.75f/3;
            var topK = 3;

            var inMemoryEmbeddingRecords = EmbeddingRecord.FromJsonFile(@".\Revolver.json");

            while (true)
            {
                var criteria = Console.ReadLine().Trim();
                if (criteria == "exit" || criteria == "quit")
                    break;

                if (!criteria.IsNullOrEmpty())
                {
                    var response = client.SimilaritySearch(index, criteria, topK, minimumScore: minimumScore);
                    foreach (var r in response.matches)
                        WriteAnswer($"Id: {r.id}, {r.score}");
                    Console.WriteLine($"");

                    var inMemoryResponse = SimilaritySearchEngine.SimilaritySearch(client.LastQuery, inMemoryEmbeddingRecords, topK, minimumScore);
                    foreach (var r in inMemoryResponse)
                        WriteAnswer($"Id: {r.Id}, {r.Score}");
                    Console.WriteLine($"");
                }

                WriteQuestion(message);
            }
        }
    }
}
