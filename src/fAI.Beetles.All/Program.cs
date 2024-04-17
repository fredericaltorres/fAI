using System;
using DynamicSugar;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace fAI.Beetles.All
{
    internal class Program
    {
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
            var sleepTime = r.Next(1000, 6000);
            System.Threading.Thread.Sleep(sleepTime);
        }

        const string JsonOutputFilename = @".\Beatles.All.json";

        private static List<EmbeddingSongRecord> LoadEmbeddingSongRecord()
        {
            var r = new List<EmbeddingSongRecord>();
            if (File.Exists(JsonOutputFilename))
                r.AddRange(EmbeddingRecord.FromJsonFile(JsonOutputFilename).Select(rr => rr as EmbeddingSongRecord));
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

        static void Main(string[] args)
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
                    album = RemoveParenthesis(v);
                    year = ExtractYearInParenthesis(v);
                    Trace(line);
                    continue;
                }

                if (line.StartsWith("/lyrics/"))
                {
                    var parts = line.Split(',');
                    var url = $"{webSiteRootUrl}{parts[0]}";
                    var songTitle = parts[2];
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
    }
}
