using DynamicSugar;
using Markdig;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace fAI
{
    // https://github.com/xoofx/markdig?tab=readme-ov-file
    // https://xoofx.github.io/markdig/
    public class MarkdownManager
    {
        static TestFileHelper _testFileHelper = new TestFileHelper();

        public static string HtmlTemplate03 = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
<!-- HtmlTemplate03 -->
<meta charset=""UTF-8"">
<style>
    @import url('https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,400;0,700;1,400&family=Source+Serif+4:ital,opsz,wght@0,8..60,300;0,8..60,400;1,8..60,300&family=JetBrains+Mono:wght@400;500&display=swap');

    :root {
        --ink:       #1a1612;
        --ink-muted: #6b5e52;
        --paper:     #faf7f2;
        --paper-alt: #f0ebe2;
        --rule:      #d8cfc4;
        --accent:    #c0522a;
        --accent-bg: #f9ede7;
    }

    * { box-sizing: border-box; }

    body {
        font-family: 'Source Serif 4', Georgia, serif;
        font-size: 17px;
        font-weight: 300;
        line-height: 1.8;
        max-width: 720px;
        margin: 48px auto;
        padding: 0 24px 80px;
        color: var(--ink);
        background-color: var(--paper);
    }

    h1, h2, h3, h4 {
        font-family: 'Playfair Display', Georgia, serif;
        font-weight: 700;
        line-height: 1.2;
        color: var(--ink);
        margin: 2.4em 0 0.6em;
    }

    h1 {
        font-size: 2.6em;
        letter-spacing: -0.02em;
        border-bottom: 2px solid var(--ink);
        padding-bottom: 0.3em;
        margin-top: 0;
    }

    h2 {
        font-size: 1.75em;
        letter-spacing: -0.01em;
        position: relative;
        padding-left: 1rem;
    }
    h2::before {
        content: '';
        position: absolute;
        left: 0; top: 0.15em; bottom: 0.15em;
        width: 3px;
        background: var(--accent);
        border-radius: 2px;
    }

    h3 {
        font-size: 1.25em;
        font-style: italic;
        font-weight: 400;
        color: var(--ink-muted);
    }

    p {
        margin: 0 0 1.2em;
        font-size: 1rem;
        hanging-punctuation: first last;
    }

    a {
        color: var(--accent);
        text-decoration: underline;
        text-decoration-thickness: 1px;
        text-underline-offset: 3px;
        transition: opacity 0.15s;
    }
    a:hover { opacity: 0.7; }

    strong { font-weight: 700; }
    em     { font-style: italic; color: var(--ink-muted); }

    hr {
        border: none;
        border-top: 1px solid var(--rule);
        margin: 2.5em 0;
    }

    code {
        font-family: 'JetBrains Mono', 'Courier New', monospace;
        font-size: 0.82em;
        background: var(--accent-bg);
        color: var(--accent);
        padding: 2px 7px;
        border-radius: 4px;
        border: 1px solid #e8c9bd;
    }

    pre {
        background: #1e1b18;
        color: #e8e0d5;
        border-radius: 8px;
        padding: 1.4em 1.6em;
        overflow-x: auto;
        font-size: 0.85em;
        line-height: 1.65;
        border-left: 3px solid var(--accent);
        margin: 1.8em 0;
    }
    pre code {
        background: none;
        color: inherit;
        padding: 0;
        border: none;
        font-size: 1em;
    }

    blockquote {
        margin: 2em 0;
        padding: 1em 1.5em;
        background: var(--paper-alt);
        border-left: 4px solid var(--rule);
        border-radius: 0 6px 6px 0;
        font-style: italic;
        color: var(--ink-muted);
    }
    blockquote p { margin: 0; }

    ul, ol {
        padding-left: 1.5em;
        margin: 0 0 1.2em;
    }
    li { margin-bottom: 0.35em; }
    li::marker { color: var(--accent); }

    table {
        width: 100%;
        border-collapse: collapse;
        margin: 1.8em 0;
        font-size: 0.93em;
    }
    th {
        font-family: 'Playfair Display', serif;
        font-weight: 700;
        text-align: left;
        border-bottom: 2px solid var(--ink);
        padding: 0.5em 0.75em;
    }
    td {
        padding: 0.5em 0.75em;
        border-bottom: 1px solid var(--rule);
    }
    tr:last-child td { border-bottom: none; }
    tr:hover td { background: var(--paper-alt); }
</style>
</head>
<body>
    [body]
</body>
</html>
";

        public static string HtmlTemplate02 = @"
        <!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
<style>
    @import url('https://fonts.googleapis.com/css2?family=Cormorant+SC:wght@300;500;600&family=Crimson+Pro:ital,wght@0,300;0,400;1,300;1,400&family=Fira+Code:wght@300;400&display=swap');

    :root {
        --void:       #0e0d0b;
        --surface:    #161410;
        --lift:       #1f1d19;
        --bone:       #ede8df;
        --bone-dim:   #9e9589;
        --gold:       #c9a84c;
        --gold-dim:   #7a6330;
        --gold-glow:  rgba(201, 168, 76, 0.12);
        --rule:       #2e2b24;
    }

    * { box-sizing: border-box; }

    body {
        font-family: 'Crimson Pro', Georgia, serif;
        font-size: 18px;
        font-weight: 300;
        line-height: 1.85;
        max-width: 740px;
        margin: 60px auto;
        padding: 0 32px 100px;
        color: var(--bone);
        background-color: var(--void);
        /* Subtle grain texture */
        background-image: url(""data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='200' height='200'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='200' height='200' filter='url(%23n)' opacity='0.03'/%3E%3C/svg%3E"");
    }

    /* ── Headings ─────────────────────────────── */
    h1, h2, h3, h4 {
        font-family: 'Cormorant SC', Georgia, serif;
        font-weight: 500;
        letter-spacing: 0.06em;
        line-height: 1.15;
        color: var(--bone);
        margin: 2.8em 0 0.7em;
    }

    h1 {
        font-size: 3em;
        font-weight: 300;
        letter-spacing: 0.12em;
        text-align: center;
        margin-top: 0;
        padding: 1.2em 0 1em;
        position: relative;
        color: var(--gold);
    }
    h1::before,
    h1::after {
        content: '';
        display: block;
        height: 1px;
        background: linear-gradient(90deg, transparent, var(--gold-dim), transparent);
        margin: 0.5em 10%;
    }

    h2 {
        font-size: 1.6em;
        color: var(--gold);
        display: flex;
        align-items: center;
        gap: 0.75em;
    }
    h2::after {
        content: '';
        flex: 1;
        height: 1px;
        background: linear-gradient(90deg, var(--gold-dim), transparent);
        opacity: 0.5;
    }

    h3 {
        font-size: 1.15em;
        font-weight: 600;
        letter-spacing: 0.15em;
        color: var(--bone-dim);
        text-transform: uppercase;
        font-variant: none;
        font-family: 'Crimson Pro', serif;
    }

    /* ── Prose ────────────────────────────────── */
    p {
        margin: 0 0 1.3em;
        font-size: 1rem;
    }

    a {
        color: var(--gold);
        text-decoration: none;
        border-bottom: 1px solid var(--gold-dim);
        padding-bottom: 1px;
        transition: border-color 0.2s, color 0.2s;
    }
    a:hover {
        color: #e2c472;
        border-color: var(--gold);
    }

    strong {
        font-weight: 600;
        color: var(--bone);
        letter-spacing: 0.01em;
    }

    em {
        font-style: italic;
        color: var(--bone-dim);
    }

    hr {
        border: none;
        margin: 3em auto;
        width: 40%;
        height: 1px;
        background: linear-gradient(90deg, transparent, var(--gold-dim), transparent);
        position: relative;
    }

    /* ── Code ─────────────────────────────────── */
    code {
        font-family: 'Fira Code', 'Courier New', monospace;
        font-size: 0.8em;
        font-weight: 300;
        background: var(--lift);
        color: var(--gold);
        padding: 2px 8px;
        border-radius: 3px;
        border: 1px solid var(--rule);
        letter-spacing: 0.02em;
    }

    pre {
        background: var(--surface);
        border: 1px solid var(--rule);
        border-top: 1px solid var(--gold-dim);
        border-radius: 4px;
        padding: 1.6em 2em;
        overflow-x: auto;
        font-size: 0.83em;
        line-height: 1.7;
        margin: 2em 0;
        position: relative;
    }
    pre::before {
        content: '';
        position: absolute;
        top: 0; left: 2em; right: 2em;
        height: 1px;
        background: linear-gradient(90deg, transparent, var(--gold), transparent);
        opacity: 0.4;
    }
    pre code {
        background: none;
        color: #c8c0b0;
        padding: 0;
        border: none;
        font-size: 1em;
    }

    /* ── Blockquote ───────────────────────────── */
    blockquote {
        margin: 2.5em 0;
        padding: 1.4em 2em;
        background: var(--gold-glow);
        border-left: 2px solid var(--gold);
        position: relative;
        font-style: italic;
        color: var(--bone-dim);
        font-size: 1.1em;
        line-height: 1.7;
    }
    blockquote::before {
        content: '\201C';
        font-family: 'Cormorant SC', serif;
        font-size: 4em;
        color: var(--gold-dim);
        position: absolute;
        top: -0.1em;
        left: 0.25em;
        line-height: 1;
        opacity: 0.5;
    }
    blockquote p {
        margin: 0;
        padding-left: 1.2em;
    }

    /* ── Lists ────────────────────────────────── */
    ul, ol {
        padding-left: 0;
        margin: 0 0 1.3em;
        list-style: none;
    }
    li {
        margin-bottom: 0.5em;
        padding-left: 1.6em;
        position: relative;
    }
    ul li::before {
        content: '◆';
        position: absolute;
        left: 0;
        color: var(--gold);
        font-size: 0.45em;
        top: 0.7em;
    }
    ol {
        counter-reset: items;
    }
    ol li::before {
        counter-increment: items;
        content: counter(items, upper-roman);
        position: absolute;
        left: 0;
        color: var(--gold-dim);
        font-family: 'Cormorant SC', serif;
        font-size: 0.8em;
        top: 0.2em;
        letter-spacing: 0.05em;
    }

    /* ── Table ────────────────────────────────── */
    table {
        width: 100%;
        border-collapse: collapse;
        margin: 2.2em 0;
        font-size: 0.92em;
    }
    thead tr {
        border-bottom: 1px solid var(--gold-dim);
    }
    th {
        font-family: 'Cormorant SC', serif;
        font-weight: 500;
        letter-spacing: 0.1em;
        color: var(--gold);
        text-align: left;
        padding: 0.6em 1em;
        font-size: 0.9em;
    }
    td {
        padding: 0.6em 1em;
        border-bottom: 1px solid var(--rule);
        color: var(--bone-dim);
        transition: color 0.15s;
    }
    tr:hover td {
        color: var(--bone);
        background: var(--gold-glow);
    }
    tr:last-child td { border-bottom: none; }
</style>    
</head>
<body>
    [body]
</body>
</html>
";
        public static string HtmlTemplate01 = @"
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
            margin: 10px 10px 10px 10px;
            color: #000000;
            background-color: #f5f5f5;
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

        public static string RemoveMarkDownTick(string text)
        {
            var marker1 = "```markdown";
            if (text.StartsWith(marker1))
                text = text.Substring(marker1.Length);
            var marker2 = "```";
            if (text.EndsWith(marker2))
                text = text.Substring(0, text.Length - marker2.Length);
            return text.Trim();
        }

        public static (string htmlFileName, string markDown) ConvertToHtmlFile(string markdown, bool openInBrowser = false, string htmlTemplate = null, string tempHtmlFile = null)
        {
            markdown = RemoveMarkDownTick(markdown);
            if(tempHtmlFile == null)
                tempHtmlFile = Path.Combine(Path.GetTempPath(), "fAI."+ (Guid.NewGuid().ToString()) + ".html");

            if (htmlTemplate == null)
                htmlTemplate = HtmlTemplate01;

            var html = htmlTemplate.Replace("[body]", ConvertToHtml(markdown));
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

        public static string ExtractStyleBlock(string htmlStr)
        {
            if (string.IsNullOrWhiteSpace(htmlStr))
                return null;

            string pattern = @"<style\s+type=""text/css""[\s\S]*?</style>";

            Match match = Regex.Match(htmlStr, pattern, RegexOptions.IgnoreCase);

            return match.Success ? match.Value : null;
        }

        /// <summary>
        /// Extracts the full <body>...</body> block from an HTML string.
        /// </summary>
        /// <param name="htmlStr">The HTML string to search.</param>
        /// <returns>The full body block including tags, or null if not found.</returns>
        public static string ExtractBodyBlock(string htmlStr)
        {
            if (string.IsNullOrWhiteSpace(htmlStr))
                return null;

            string pattern = @"<body[\s\S]*?</body>";

            Match match = Regex.Match(htmlStr, pattern, RegexOptions.IgnoreCase);

            return match.Success ? match.Value : null;
        }

        //public static void ConvertCodeToHtmlFile(string code, string syntaxFile, string themeFile, string htmlFile)
        //{
        //    var (html, _, __) = ConvertCodeToHtml(code, syntaxFile, themeFile);
        //    File.WriteAllText(htmlFile, html);
        //}

        //// https://github.com/smdn/Smdn.LibHighlightSharp/tree/main
        //public static (string html, string style, string body)ConvertCodeToHtml(string code, string syntaxFile, string themeFile)
        //{
        //    // Creates an instance that generates code highlighted as a HTML document.
        //    using (var hl = new Highlight(GeneratorOutputType.Html))
        //    {
        //        hl.SetThemeFromFile(themeFile);
        //        hl.SetSyntaxFromFile(syntaxFile);
        //        hl.SetIncludeStyle(true);
        //        var html = hl.Generate(code);

        //        return (html, ExtractStyleBlock(html) , ExtractBodyBlock(html));
        //    }
        //}
    }
}
