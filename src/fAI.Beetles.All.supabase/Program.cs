using fAI;
using Newtonsoft.Json;
using Supabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

/*
    https://themurph.hashnode.dev/supabase-csharp
    https://supabase.com/docs/reference/csharp/using-modifiers

 */
namespace SupabaseThoughts
{
    class Program
    {
        private const string ProjectUrl = "https://qqxkpjxwutfhvzwywbdl.supabase.co";
        private const string AnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InFxeGtwanh3dXRmaHZ6d3l3YmRsIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzI1NTczOTgsImV4cCI6MjA4ODEzMzM5OH0.qW7ZMsoT28CEIzBmvjIjZHb86bDjkirwsP5uLMLvhyE";

        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();
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
        static void WriteInformation(string message) => WriteLine(message, ConsoleColor.White);
        static void WriteAnswer(string message) => WriteLine(message, ConsoleColor.Green);

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

        static void Assert(bool exp, string message)
        {
            if(!exp)
            {
                throw new ApplicationException(message);
            }
        }

        public class BeatlesSongResult
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("similarity")]
            public float Similarity { get; set; }

            // Add other columns your function returns
        }

        static async Task RunAsync()
        {
            Console.WriteLine("=== Supabase BeatlesSongs Console App ===");
            Console.WriteLine();

            var supabase = new Supabase.Client(ProjectUrl, AnonKey, new Supabase.SupabaseOptions { AutoConnectRealtime = true });
            await supabase.InitializeAsync();

            // await PopulateBeastleSongsTableInSupabase(supabase);


            var embeddingSongRecords = EmbeddingRecord.LoadEmbeddingSongRecord();

            var albums = embeddingSongRecords.Select(r => $"{r.Year} - {r.Album}").ToList().Distinct().OrderBy(a => a).ToList();
            var embeddingRecords = embeddingSongRecords.Select(e => e as EmbeddingRecord).ToList();

            var message = $"{embeddingSongRecords.Count} songs loaded. Enter search criteria about the Beatles lyrics.";
            WriteQuestion(message);
            WriteInformation("Enter 'exit' to quit.");

            var minimumScore = -1.0; // with new model score does not count
            var topK = 10;

            while (true)
            {
                var criteria = Console.ReadLine().Trim();
                if (criteria == "exit" || criteria == "quit")
                    break;

                if (!string.IsNullOrEmpty( criteria))
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "query_embedding", ToVector(criteria) },
                        { "match_threshold", 0.2f },
                        { "match_count", 20 }
                    };
                    var response = await supabase.Rpc("search_beatles_songs", parameters);
                    var s = response.Content;
                    var inMemoryResponse = JsonConvert.DeserializeObject<List<BeatlesSongResult>>(s);

                    var bestScore = inMemoryResponse.Select(r => r.Similarity).DefaultIfEmpty(0).Max();
                    minimumScore = bestScore * 0.80f;
                    inMemoryResponse = inMemoryResponse.Where(r => r.Similarity >= minimumScore).ToList();

                    Console.WriteLine($"bestScore: {bestScore}, minimumScore: {minimumScore}");
                    foreach (var r in inMemoryResponse)
                        WriteAnswer($"Id: {r.Id}, {r.Similarity:0.0000}");
                    Console.WriteLine($"");
                }
                WriteQuestion(message);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task PopulateBeastleSongsTableInSupabase(Supabase.Client supabase)
        {
            var result = await supabase.From<BeatlesSongs>().Get();
            foreach (var song in result.Models)
            {
                await supabase.From<BeatlesSongs>().Delete(song);
            }
            var embeddingSongRecords = EmbeddingRecord.LoadEmbeddingSongRecord();
            foreach (var r in embeddingSongRecords)
            {
                Console.WriteLine($"Inserting {r.Title} from {r.Album} ({r.Year})...");
                var song = new BeatlesSongs(r.Id, r.Title, r.Album, r.Year, r.Text, r.Embedding);
                var rrr4 = await supabase.From<BeatlesSongs>().Insert(song);
                Assert(rrr4.Models.Count == 1, $"Insert succeeded for {r.Title}");
            }
        }
    }
}
