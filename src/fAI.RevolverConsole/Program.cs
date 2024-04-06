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
            while(true)
            {
                var criteria = Console.ReadLine().Trim();
                if (criteria == "exit")
                    break;

                if (!criteria.IsNullOrEmpty())
                {
                    var client = new PineconeDB();
                    var index = client.GetIndex(BeatlesRevolverIndexName);
                    var minimumScore = 0.75f;
                    var response = client.SimilaritySearch(index, criteria, 3, minimumScore: minimumScore);
                    foreach (var r in response.matches)
                        WriteAnswer($"Id: {r.id}, {r.score}");
                    Console.WriteLine($"");
                }
                WriteQuestion(message);
            }
        }
    }
}
