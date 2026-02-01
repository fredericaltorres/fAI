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
            ///WebScrapLyrics();
            ComputeEmbedding();
            Environment.Exit(0);    
            var embeddingSongRecords = LoadEmbeddingSongRecord();

            var albums = embeddingSongRecords.Select(r => $"{r.Year} - {r.Album}").ToList().Distinct().OrderBy(a => a).ToList();

            var embeddingRecords = embeddingSongRecords.Select(e => e as EmbeddingRecord).ToList();

            var message = $"{embeddingSongRecords.Count} songs loaded. Enter search criteria about the Beatles lyrics.";
            WriteQuestion(message);
            WriteInformation("Enter 'exit' to quit.");

            var minimumScore = 0.75f;
            var topK = 3;

            while (true)
            {
                var criteria = Console.ReadLine().Trim();
                if (criteria == "exit" || criteria == "quit")
                    break;

                if (!criteria.IsNullOrEmpty())
                {
                    var inMemoryResponse = SimilaritySearchEngine.SimilaritySearch(SimilaritySearchEngine.ToVector(criteria), embeddingRecords, topK, minimumScore);
                    foreach (var r in inMemoryResponse)
                        WriteAnswer($"Id: {r.Id}, {r.Score:0.0000}");
                    Console.WriteLine($"");
                }
                WriteQuestion(message);
            }
        }

        static string ExtractTag(string line, string tag)
        {
            if (line.StartsWith(tag))
                return line.Substring(tag.Length).Trim();
            return null;
        }

        static void Trace(string message, ConsoleColor color = ConsoleColor.Cyan)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;
        }

        static string ExtractTextFromHtmlUrl(string url, string titleMarker)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var innerText = doc.DocumentNode.InnerText;

            var startPos = innerText.IndexOf(titleMarker);
            if (startPos == -1)
                return null;

            var startPos2 = innerText.IndexOf(titleMarker, startPos + 1);

            var text = innerText.Substring(startPos2);

            var endTag = "Submit Corrections";
            var endPos = text.IndexOf(endTag);

            text = text.Substring(0, endPos);

            text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            text = text.Trim();

            return text;
        }

        static void SleepForRandomTime()
        {
            var r = new Random();
            var sleepTime = r.Next(5000, 10000);
            System.Threading.Thread.Sleep(sleepTime);
        }

        const string JsonOutputFilename = @".\Beatles.All.json";

        private static List<EmbeddingSongRecord> LoadEmbeddingSongRecord()
        {
            var r = new List<EmbeddingSongRecord>();
            if (File.Exists(JsonOutputFilename))
                r.AddRange(EmbeddingSongRecord.FromJsonFile(JsonOutputFilename).Select(rr => rr as EmbeddingSongRecord));
            return r;
        }

        private static void SaveEmbeddingSongRecord(List<EmbeddingSongRecord> embeddingSongRecord) 
        { 
            if(File.Exists(JsonOutputFilename))
                File.Delete(JsonOutputFilename);
            EmbeddingRecord.ToJsonFile(embeddingSongRecord.Select(r => r as EmbeddingRecord).ToList(), JsonOutputFilename);
        }

        private static string RemoveParenthesis(string line)
        {
            var startPos = line.IndexOf('(');
            if (startPos == -1)
                return line;
            var endPos = line.IndexOf(')', startPos);
            if (endPos == -1)
                return line;
            return line.Substring(0, startPos) + line.Substring(endPos + 1);
        }

        private static int ExtractYearInParenthesis(string line)
        {
            var startPos = line.IndexOf('(');
            if (startPos == -1)
                return 0;
            var endPos = line.IndexOf(')', startPos);
            if (endPos == -1)
                return 0;
            var year = line.Substring(startPos + 1, endPos - startPos - 1);
            return int.Parse(year);
        }

        static void WebScrapLyrics()
        {
            Trace("Aquiring All Beetles Lyrics");
            var album = string.Empty;
            var year = 1960;
            var webSiteRootUrl = "https://www.azlyrics.com/lyrics";
            var v = string.Empty;
            var lines = File.ReadAllText(@".\All.Beetles.txt").SplitByCRLF();

            var embeddingSongRecord = LoadEmbeddingSongRecord();

            foreach (var line in lines)
            {
                v = ExtractTag(line, "website:");
                if (v != null)
                {
                    webSiteRootUrl = v;
                    Trace(line);
                    continue;
                }

                v = ExtractTag(line, "album:");
                if (v != null)
                {
                    album = RemoveParenthesis(v).Trim();
                    year = ExtractYearInParenthesis(v);
                    Trace(line);
                    continue;
                }

                if (line.StartsWith("/lyrics/"))
                {
                    var parts = line.Split(',');
                    var url = $"{webSiteRootUrl}{parts[0]}";
                    var songTitle = parts[2].Replace(";", ",");
                    var titleMarker = $@"""{songTitle}""";
                    var id = $"Beatles - {album} - {songTitle}";

                    var exists = embeddingSongRecord.FirstOrDefault(r => r.Id == id) != null;

                    if (!exists)
                    {
                        Trace($"Loading {url}", ConsoleColor.White);
                        var text = ExtractTextFromHtmlUrl(url, titleMarker);

                        if (text == null)
                            Trace($"Failed to load {url}", ConsoleColor.Red);

                        embeddingSongRecord.Add(new EmbeddingSongRecord
                        {
                            Id = id,
                            Year = year,
                            Album = album,
                            Title = songTitle,
                            Text = text,
                            Embedding = new List<float>()
                        });

                        SaveEmbeddingSongRecord(embeddingSongRecord);
                        SleepForRandomTime();
                    }
                    continue;
                }
            }
            
            Trace("Done");
            Console.ReadLine();
        }

        static void ComputeEmbedding()
        {
            ///WebScrapLyrics();
            var embeddingSongRecords = LoadEmbeddingSongRecord();

            //foreach (var e in embeddingSongRecords)
            //    e.Embedding.Clear();
            //SaveEmbeddingSongRecord(embeddingSongRecords);

            var client = new OpenAI();
            var i = 0;
            foreach (var e in embeddingSongRecords)
            {
                Console.WriteLine($"{i} - {e.Album} - {e.Title}");
                if (e.Embedding == null || e.Embedding.Count == 0)
                {
                    var r = client.Embeddings.Create(e.Text);
                    e.Embedding = r.Data[0].Embedding;

                    if (i++ % 10 == 0)
                        SaveEmbeddingSongRecord(embeddingSongRecords);
                }
            }
            SaveEmbeddingSongRecord(embeddingSongRecords);
        }
    }
}
