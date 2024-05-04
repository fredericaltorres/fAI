using System;
using DynamicSugar;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using static DynamicSugar.DS;
using fAI.VectorDB;

namespace fAI.Beetles.All
{
    internal class Program
    {
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

        static void Main(string[] args)
        {
            Console.Clear();
            var embeddingSourceCodeRecords = LoadEmbeddingSourceCodeRecord();

            //var news = EmbeddingSourceCodeRecord.LoadSourceCodeList(@"c:\a\monitors.txt", 5);
            //news.ForEach(n => n.Project = $"Monitor\\{n.Project}");
            //embeddingSourceCodeRecords.AddRange(news);
            //SaveEmbeddingSourceCodeRecord(embeddingSourceCodeRecords);
            //ComputeEmbedding();
            //Environment.Exit(0);

            var embeddingRecords = embeddingSourceCodeRecords.Select(e => e as EmbeddingRecord).ToList();
            var message = $"{embeddingSourceCodeRecords.Count} Source File / Chunk . Enter search criteria about the source code";
            WriteQuestion(message);
            WriteInformation("Enter 'exit' to quit.");

            var minimumScore = 0.75f;
            var topK = 10;

            while (true)
            {
                var criteria = Console.ReadLine().Trim();
                if (criteria == "exit" || criteria == "quit")
                    break;
                if (criteria == "cls")
                {
                    Console.Clear();
                    continue;
                }

                if (!criteria.IsNullOrEmpty())
                {
                    var inMemoryResponse = SimilaritySearchEngine.SimilaritySearch(SimilaritySearchEngine.ToVector(criteria), embeddingRecords, topK, minimumScore);
                    foreach (var r in inMemoryResponse)
                    {
                        var rr = r as EmbeddingSourceCodeRecord;
                        WriteAnswer($"Id: {r.Id}, {r.Score:0.0000},  Prj: {rr.Project}, File: {rr.ShorterFileName(4)} {rr.TextLengthKb} Kb, Chunk: {rr.ChunkIndex}");
                    }
                    Console.WriteLine($"");
                }
                WriteQuestion(message);
            }
        }

        static void Trace(string message, ConsoleColor color = ConsoleColor.Cyan)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }

        static void SleepForRandomTime()
        {
            var r = new Random();
            var sleepTime = r.Next(5000, 10000);
            System.Threading.Thread.Sleep(sleepTime);
        }

        const string JsonOutputFilename = @".\source.code.json";

        private static List<EmbeddingSourceCodeRecord> LoadEmbeddingSourceCodeRecord()
        {
            var r = new List<EmbeddingSourceCodeRecord>();
            if (File.Exists(JsonOutputFilename))
                r.AddRange(EmbeddingSourceCodeRecord.FromJsonFile(JsonOutputFilename).Select(rr => rr as EmbeddingSourceCodeRecord));
            return r;
        }

        private static void SaveEmbeddingSourceCodeRecord(List<EmbeddingSourceCodeRecord> embeddingSongRecord) 
        { 
            if(File.Exists(JsonOutputFilename))
                File.Delete(JsonOutputFilename);
            EmbeddingRecord.ToJsonFile(embeddingSongRecord.Select(r => r as EmbeddingRecord).ToList(), JsonOutputFilename);
        }

        static void ComputeEmbedding()
        {
            ///WebScrapLyrics();
            var embeddingSourceRecords = LoadEmbeddingSourceCodeRecord();
            Console.WriteLine($"ComputeEmbedding {embeddingSourceRecords.Count} to compute");
            var client = new OpenAI();
            var i = embeddingSourceRecords.Where(e => e.Embedding == null).ToList().Count;
            foreach (var e in embeddingSourceRecords)
            {
                Console.WriteLine($"{i} - {e.Project} - {e.FileNameOnly} - Chunk: {e.ChunkIndex}");
                if (e.Embedding == null || e.Embedding.Count == 0)
                {
                    var r = client.Embeddings.Create(e.Text);
                    e.Embedding = r.Data[0].Embedding;

                    if (i++ % 100 == 0)
                        SaveEmbeddingSourceCodeRecord(embeddingSourceRecords);
                }
            }
            SaveEmbeddingSourceCodeRecord(embeddingSourceRecords);
        }
    }
}
