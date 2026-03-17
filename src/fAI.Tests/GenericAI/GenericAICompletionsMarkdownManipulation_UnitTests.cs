using DynamicSugar;
using fAI;
using Markdig;
using Newtonsoft.Json;
//using Smdn.LibHighlightSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Xunit;
using static fAI.HumeAISpeech;
using static fAI.OpenAICompletions;
using static fAI.OpenAICompletionsEx;
using static System.Net.Mime.MediaTypeNames;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class GenericAICompletionsMarkdownManipulation_UnitTests : OpenAIUnitTestsBase, IDisposable
    {
        public GenericAICompletionsMarkdownManipulation_UnitTests()
        {
            OpenAI.TraceOn = true;
        }


        const string markDownContentTestPlan1 = @"
# Test Plans

## Module Test Plans

| # | Module Test Plan |
|---|-----------|
| 1 | LearningReminderMonitor |
| 2 | BatchAdminMonitor |
| 3 | BatchAdminMonitorJob |
| 4 | BatchAdminMonitorUtils |
| 5 | CertificateConverterMonitor |
| 6 | ExpirationMonitor |
| 7 | LearningBadgeLogic  |
| 8 | SFDCScoringMonitor |
| 9 | SFDCUserSyncMonitor (email feature disabled) |
| 10 | UnzipScormMonitor  |
| 11 | SlideAudioConverterContext |
| 12 | ViewEndMonitor |
| 13 | DeliveryMonitor (EmailDeliveryHandler) |
";

        const string MarkDownEditorSystemPrompt = @"
            You are a markdown editor.
            Your job is to apply the requested changes to the markdown content and return ONLY the updated markdown 
            with no explanation or commentary.

            Comment should always be in italic and in parenthesis.

            When marking an item as ""done"", add a ✅ emoji at the end of that row.
            When marking an item with some ""issue"", add a ❌ emoji at the end of that row.
            When marking an item with ""warning"", add a ⚠️ emoji at the end of that row.

            Return ONLY the updated markdown content.
            ";

        [Fact()]
        [TestBeforeAfter]
        public void Conversation_GenericAI_MarkDownManipulation()
        {
            var instructions = DS.List(
                "Add comment to test plan 1, 'Very high priority'",
                "mark test plan 13 with a warning",
                "mark test plan 2 as done",
                "mark test plan 3 with an error"
                );

            var markdownContent = markDownContentTestPlan1;
            var htmlMarkDownFileName = "";

            foreach (var instruction in instructions)
            {
                var prompt = $@"
                    Instruction: {instruction}.
                    Markdown Content:
                    {markdownContent}";

                var models = DS.List("gemini-2.0-flash"); //, "claude-haiku-4-5" // , "claude-sonnet-4-5", , "gpt-5-mini"
                //var models = DS.List("claude-haiku-4-5"); //, "claude-haiku-4-5" // , "claude-sonnet-4-5", , "gpt-5-mini"
                foreach (var model in models)
                {
                    var sw = Stopwatch.StartNew();
                    var client = new GenericAI();
                    var (text, contents) = client.Completions.Create(prompt, model: model, systemPrompt: MarkDownEditorSystemPrompt);
                    sw.Stop();
                    (htmlMarkDownFileName, markdownContent) = MarkdownManager.ConvertToHtmlFile(text, true);
                    HttpBase.Trace($"[CONVERSATION] Model: {model}, Duration: {sw.ElapsedMilliseconds / 1000.0:0.0}, Response: {text}", this);
                }
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void Markdown_Generation_2()
        {
            var text = markDownContentTestPlan1;
            var htmlMarkDown = MarkdownManager.ConvertToHtmlFile(text, true);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Markdown_Generation_1()
        {
            var text = @"
# Issue Icon Reference

## Status Icons

| Icon | Code | Usage |
|------|------|-------|
| ❌ | `:x:` | Broken, failed, or blocked |
| ⚠️ | `:warning:` | Needs attention, non-critical |
| 🔴 | `:red_circle:` | Critical issue |
| 🟡 | `:yellow_circle:` | Warning / in progress |
| 🟢 | `:green_circle:` | All good |
| ❗ | `:exclamation:` | Urgent issue |
";
            var htmlMarkDown = MarkdownManager.ConvertToHtmlFile(text, true);
        }

        const string BASIC_MARKDOWN_1 = @"
# Markdown Showcase

This file demonstrates common **Markdown features**.

---

## 1. Headings

# Heading 1
## Heading 2
### Heading 3
#### Heading 4
##### Heading 5
###### Heading 6

---

## 2. Text Formatting

- **Bold text**
- *Italic text*
- ***Bold and italic***
- ~~Strikethrough~~
- `Inline code`

---

## 3. Lists

### Unordered list

- Item 1
- Item 2
  - Sub item 2.1
  - Sub item 2.2

### Ordered list

1. First
2. Second
3. Third

---

## 4. Links

[OpenAI](https://openai.com)

---

## 5. Images

![Sample Image](https://via.placeholder.com/150)

---

## 6. Code blocks

### C#

```csharp
public static void Main()
{
    Console.WriteLine(""Hello Markdown"");
}
```

## 7. Table

| Icon | Code | Usage |
|------|------|-------|
| ❌ | `:x:` | Broken, failed, or blocked |
| ⚠️ | `:warning:` | Needs attention, non-critical |
| 🔴 | `:red_circle:` | Critical issue |
| 🟡 | `:yellow_circle:` | Warning / in progress |
| 🟢 | `:green_circle:` | All good |
| ❗ | `:exclamation:` | Urgent issue |
";

        [Fact()]
        [TestBeforeAfter]
        public void Basic_Markdown_Generation_1()
        {
            var htmlMarkDown = MarkdownManager.ConvertToHtmlFile(BASIC_MARKDOWN_1, openInBrowser: true, htmlTemplate: MarkdownManager.HtmlTemplate01);
            htmlMarkDown = MarkdownManager.ConvertToHtmlFile(BASIC_MARKDOWN_1, openInBrowser: true, htmlTemplate: MarkdownManager.HtmlTemplate02);
            htmlMarkDown = MarkdownManager.ConvertToHtmlFile(BASIC_MARKDOWN_1, openInBrowser: true, htmlTemplate: MarkdownManager.HtmlTemplate03);
        }

        const string github_theme = @"C:\DVT\fAI\src\fAI.Tests\bin\Debug\highlight\themes\github.theme";
        const string csharp_lang = @"C:\DVT\fAI\src\fAI.Tests\bin\Debug\highlight\langDefs\csharp.lang";

        const string CSCHARP_CODE_SAMPLE_1 = @"
/// <summary>
/// Extracts the full <body>...</body> block from an HTML string.
/// </summary>
/// <param name=""htmlStr"">The HTML string to search.</param>
/// <returns>The full body block including tags, or null if not found.</returns>
public static string ExtractBodyBlock(string htmlStr)
{
    if (string.IsNullOrWhiteSpace(htmlStr))
        return null;
    string pattern = @""<body[\s\S]*?</body>"";
    Match match = Regex.Match(htmlStr, pattern, RegexOptions.IgnoreCase);
    return match.Success ? match.Value : null;
}
";



/*
        [Fact()]
        [TestBeforeAfter]
        public void RenderCSharp_1()
        {
            var (html, style, body)  = MarkdownManager.ConvertCodeToHtml(CSCHARP_CODE_SAMPLE_1, csharp_lang, github_theme);
            var tfh = new TestFileHelper();
            var tempHtmlFile = tfh.GetTempFileName(".html");
            MarkdownManager.ConvertCodeToHtmlFile(CSCHARP_CODE_SAMPLE_1, csharp_lang, github_theme, tempHtmlFile);
            Assert.True(File.Exists(tempHtmlFile), "HTML file was not created.");
        }

        [Fact()]
        [TestBeforeAfter]
        public void Markdown_Generation_With_CSharpCode()
        {
            var text = @"
# Some markdown with some c#

## Overview
About this code.

## Sample code

```csharp
public static string ExtractBodyBlock(string htmlStr)
{
    string pattern = @""<body[\s\S]*?</body>"";
    Match match = Regex.Match(htmlStr, pattern, RegexOptions.IgnoreCase);
    return match.Success ? match.Value : null;
}

```
";
            // var htmlMarkDown = MarkdownManager.ConvertToHtmlFile(text, true);
        }
*/
        public void Dispose()
        {
            // MarkdownManager.Clean();
        }
    }
}