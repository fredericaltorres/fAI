using DynamicSugar;
using Markdig;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace fAI
{
    // https://github.com/xoofx/markdig?tab=readme-ov-file
    // https://xoofx.github.io/markdig/
    public class MarkdownManager
    {
        static TestFileHelper _testFileHelper = new TestFileHelper();

        public static void Clean()
        {
            _testFileHelper.Clean();
        }

        public static void OpenHtmlFileInBrowser(string htmlFile)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(htmlFile) { UseShellExecute = true });
        }

        public static string ConvertToHtmlFile(string markdown, bool openInBrowser = false)
        {
            var tempHtmlFile = Path.Combine(Path.GetTempPath(), "fAI."+ (Guid.NewGuid().ToString()) + ".html");
            File.WriteAllText(tempHtmlFile, ConvertToHtml(markdown));

            _testFileHelper.TrackFile(tempHtmlFile);

            if (openInBrowser)
                OpenHtmlFileInBrowser(tempHtmlFile);
            return tempHtmlFile;
        }

        public static string ConvertToHtml(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            string html = Markdown.ToHtml(markdown, pipeline);
            return html;
        }
    }
}
