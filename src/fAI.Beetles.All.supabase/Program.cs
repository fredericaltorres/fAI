using fAI;
using fAI.VectorDB;
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
        private static string AnonKey => Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY") ?? throw new ApplicationException("SUPABASE_ANON_KEY environment variable is not set.");

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

        const string JsonOutputFilename = @".\Beatles.All.json";

        static async Task RunAsync()
        {
            
            WriteQuestion("=== Supabase BeatlesSongs Console App ===");

            var supabase = new Supabase.Client(ProjectUrl, AnonKey, new Supabase.SupabaseOptions { AutoConnectRealtime = true });
            await supabase.InitializeAsync();

            //await PopulateBeatlesSongsTableInSupabase(supabase);

            var embeddingSongRecords = EmbeddingSongRecord.LoadEmbeddingSongRecord(JsonOutputFilename);
            var albums = embeddingSongRecords.Select(r => $"{r.Year} - {r.Album}").ToList().Distinct().OrderBy(a => a).ToList();
            var embeddingRecords = embeddingSongRecords.Select(e => e as EmbeddingCommonRecord).ToList();

            var message = $"{embeddingSongRecords.Count} songs loaded. Enter search criteria about the Beatles lyrics.";
            WriteQuestion(message);
            WriteInformation("Enter 'exit' to quit.");

            var minimumScoreInSupaBase = 0.2f; // with new model score does not count
            var minimumScore = -1.0; // with new model score does not count

            while (true)
            {
                var criteria = Console.ReadLine().Trim();
                if (criteria == "exit" || criteria == "quit")
                    break;

                if (criteria == "cls")
                {
                    Console.Clear();
                    WriteQuestion(message);
                    continue;
                }

                if (!string.IsNullOrEmpty(criteria))
                {
                    var swTotalTime = System.Diagnostics.Stopwatch.StartNew();
                    var swQueryVector = System.Diagnostics.Stopwatch.StartNew();
                    var queryVector = ToVector(criteria);
                    swQueryVector.Stop();

                    var parameters = new Dictionary<string, object>
                    {
                        { "query_embedding",  queryVector},
                        { "match_threshold", minimumScoreInSupaBase },
                        { "match_count", 20 }
                    };
                    var swSimilaritySearch = System.Diagnostics.Stopwatch.StartNew();
                    var response = await supabase.Rpc("search_beatles_songs", parameters);
                    swSimilaritySearch.Stop();

                    var s = response.Content;
                    var inMemoryResponse = JsonConvert.DeserializeObject<List<BeatlesSongResult>>(s);
                    var inMemoryResponse2 = inMemoryResponse;

                    // tutu Dynamic Thresholding based on best score. This is needed as the new model return variable score
                    // and we need to relax the threshold to get relevant results.
                    // With old model, we can set a fixed threshold and get good results.
                    var bestScore = inMemoryResponse.Select(r => r.Similarity).DefaultIfEmpty(0).Max();
                    minimumScore = SimilaritySearchEngine.GetOpenAIEmbeddingDynamicScore(bestScore);

                    inMemoryResponse = inMemoryResponse.Where(r => r.Similarity >= minimumScore).ToList();

                    swTotalTime.Stop();

                    Console.WriteLine($"bestScore: {bestScore}, minimumScore: {minimumScore}, minimumScoreInSupaBase: {minimumScoreInSupaBase}, RecordReturned: {inMemoryResponse2.Count}");
                    Console.WriteLine($"Query Vector creation Time: {swQueryVector.ElapsedMilliseconds/1000.0}, Similarity Search Time: {swSimilaritySearch.ElapsedMilliseconds/100.0}, Total Time: {(swQueryVector.ElapsedMilliseconds+ swSimilaritySearch.ElapsedMilliseconds) / 1000.0}, Total Time2: {swTotalTime.ElapsedMilliseconds / 1000.0}");
                    var index = 0;
                    foreach (var r in inMemoryResponse)
                    {
                        WriteAnswer($"[{index++}]Id: {r.Id}, {r.Similarity:0.0000}");
                    }
                    Console.WriteLine($"--");
                }
                WriteQuestion(message);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task PopulateBeatlesSongsTableInSupabase(Supabase.Client supabase)
        {
            var result = await supabase.From<BeatlesSongs>().Get();
            foreach (var song in result.Models)
            {
                await supabase.From<BeatlesSongs>().Delete(song);
            }
            var embeddingSongRecords = EmbeddingSongRecord.LoadEmbeddingSongRecord(JsonOutputFilename);
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
