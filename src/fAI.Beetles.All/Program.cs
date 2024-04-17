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

        static void Trace (string message, ConsoleColor color = ConsoleColor.Cyan)
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
            if(startPos == -1)
                return null;

            var startPos2 = innerText.IndexOf(titleMarker, startPos+1);

            var text = innerText.Substring(startPos2);

            var endTag = "Submit Corrections";
            var endPos = text.IndexOf(endTag);

            text = text.Substring(0, endPos);

            text = text.Replace(Environment.NewLine+ Environment.NewLine, Environment.NewLine);
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

        static void Main(string[] args)
        {
            Trace("Aquiring All Beetles Lyrics");
            var album = string.Empty;
            var webSiteRootUrl = "https://www.azlyrics.com/lyrics";
            var v = string.Empty;
            var lines = File.ReadAllText(@".\All.Beetles.txt").SplitByCRLF();

            var embeddingSongRecord = new List<EmbeddingSongRecord>();

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
                    album = v;
                    Trace(line);
                    continue;
                }

                if (line.StartsWith("/lyrics/"))
                {
                    var parts = line.Split(',');
                    var url = $"{webSiteRootUrl}{parts[0]}";
                    var songTitle = parts[2];
                    var titleMarker = $@"""{songTitle}""";
                    Trace($"Loading {url}", ConsoleColor.White);
                    var text = ExtractTextFromHtmlUrl(url, titleMarker);

                    if(text == null)
                        Trace($"Failed to load {url}", ConsoleColor.Red);

                    embeddingSongRecord.Add(new EmbeddingSongRecord {
                        Id = $"Beatles - {album} - {songTitle}",
                        Album = album,
                        Title = songTitle,
                        Text = text,
                        Embedding = new List<float>()
                    });

                    SleepForRandomTime();
                    continue;
                }
            }
            EmbeddingRecord.ToJsonFile(embeddingSongRecord.Select(r => r as EmbeddingRecord).ToList(), @".\Beatles.All.json");
            Trace("Done");
            Console.ReadLine();
        }
    }
}
