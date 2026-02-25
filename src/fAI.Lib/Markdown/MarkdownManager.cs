using DynamicSugar;
using Markdig;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace fAI
{
    // https://github.com/xoofx/markdig?tab=readme-ov-file
    // https://xoofx.github.io/markdig/
    public class MarkdownManager
    {
        static TestFileHelper _testFileHelper = new TestFileHelper();

        public static string HtmlTemplate = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <style>
        body {
            font-family: Consolas,'Segoe UI', Arial, sans-serif;
            font-size: 16px;
            line-height: 1.6;
            max-width: 800px;
            margin: 40px auto;
            color: #333;
        }
        h1 {
            font-size: 2em;
            font-family: Consolas, 'Georgia', serif;
        }
        h2 {
            font-size: 1.5em;
            font-family: Consolas, 'Georgia', serif;
        }
        p {
            font-size: 1rem;
        }
        code {
            font-family: Consolas, 'Courier New', monospace;
            font-size: 0.9em;
            background: #f4f4f4;
            padding: 2px 6px;
            border-radius: 4px;
        }
    </style>
</head>
<body>
    [body]
</body>
</html>
";

        public static void Clean()
        {
            _testFileHelper.Clean();
        }

        public static void OpenHtmlFileInBrowser(string htmlFile)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(htmlFile) { UseShellExecute = true });
        }

        public static string CheckForMarkDownTick(string text)
        {
            var marker1 = "```markdown";
            if (text.StartsWith(marker1))
                text = text.Substring(marker1.Length);
            var marker2 = "```";
            if (text.EndsWith(marker2))
                text = text.Substring(0, text.Length - marker2.Length);
            return text.Trim();
        }

        public static (string htmlFileName, string markDown) ConvertToHtmlFile(string markdown, bool openInBrowser = false)
        {
            markdown = CheckForMarkDownTick(markdown);
            var tempHtmlFile = Path.Combine(Path.GetTempPath(), "fAI."+ (Guid.NewGuid().ToString()) + ".html");

            var html = HtmlTemplate.Replace("[body]", ConvertToHtml(markdown));
            File.WriteAllText(tempHtmlFile, html);

            _testFileHelper.TrackFile(tempHtmlFile);

            if (openInBrowser)
                OpenHtmlFileInBrowser(tempHtmlFile);
            return (tempHtmlFile, markdown);
        }

        public static string ConvertToHtml(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            string html = Markdown.ToHtml(markdown, pipeline);
            return html;
        }
    }
}
