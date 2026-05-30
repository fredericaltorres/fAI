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
                    var (text, contents, usage) = client.Completions.Create(prompt, model: model, systemPrompt: MarkDownEditorSystemPrompt);
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
            var (htmlFile, markDown) = MarkdownManager.ConvertToHtmlFile(text);
            Assert.True(File.Exists(htmlFile), "HTML file was not created.");
            Assert.Contains("Module Test Plan", File.ReadAllText(htmlFile));
            Assert.Contains(@"font-family: Consolas,'Segoe UI', Arial, sans-serif;", File.ReadAllText(htmlFile));
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
            var (htmlFile, markDown) = MarkdownManager.ConvertToHtmlFile(text);
            Assert.True(File.Exists(htmlFile), "HTML file was not created.");
            Assert.Contains("Broken, failed, or blocked", File.ReadAllText(htmlFile));
            Assert.Contains(@"font-family: Consolas,'Segoe UI', Arial, sans-serif;", File.ReadAllText(htmlFile));
        }


        const string BASIC_MARKDOWN_WITH_CSHARP_CODE_1 = @"
# C# Color Coding 

Code sample with C# syntax highlighting:

```csharp 
public static string ExtractBodyBlock(string htmlStr)
{
    if (string.IsNullOrWhiteSpace(htmlStr))
        return null;
    string pattern = @""<body[\s\S]*?</body>"";
    Match match = Regex.Match(htmlStr, pattern, RegexOptions.IgnoreCase);
    return match.Success ? match.Value : null;
}
```

```javascript
function greet(name) {
    console.log(`Hello, ${name}!`);
}
```

```typescript
interface User {
    id: number;
    name: string;
    email: string;
}
```

End of markdown.

";

        const string BASIC_MARKDOWN_1 = @"
# Markdown Showcase

This file demonstrates common **Markdown features**.

---

## 1. Headings

\# Heading 1
\## Heading 2
\### Heading 3
\#### Heading 4
\##### Heading 5
\###### Heading 6

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


        const string BASIC_MARKDOWN_WITH_IMAGE = @"
# ShowPad / Brainshark Innovation Week 2026 - (Frederic Torres) `

![Brainshark1](https://fredcloud2026.blob.core.windows.net/public/Brainshark/Brainshark.Markdown.InnovationWeek2026.FredericTorres.1.jpg)

## Proposal

![Brainshark2](https://fredcloud2026.blob.core.windows.net/public/Brainshark/Brainshark.Markdown.InnovationWeek2026.FredericTorres.1.jpg)

";

        [Fact]
        public void SplitByHeading1and2()
        {
            var result = MarkdownManager.SplitByHeadings(BASIC_MARKDOWN_1);
            Assert.Equal(8, result.Count);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Basic_Markdown_GetImages()
        {
            var images = MarkdownManager.GetImages(BASIC_MARKDOWN_WITH_IMAGE);
            var imageFile = images.First().DownloadImage();
        }

        [Fact()]
        [TestBeforeAfter]
        public void Basic_Markdown_ToText()
        {
            var textMarkDown = MarkdownManager.ConvertToText(BASIC_MARKDOWN_1);
        }

        [Fact]
        public void ExtractTitle_WithValidH1Heading_ReturnsTitle()
        {
            var markdown = "# My Blog Post\nThis is some content.";
            var result = MarkdownManager.ExtractTitle(markdown);

            Assert.Equal("My Blog Post", result);
        }

        [Fact]
        public void ExtractTitle_WithMultipleHeadings_ReturnsFirstH1()
        {
            var markdown = "## Section\n# Main Title\n# Another Title";
            var result = MarkdownManager.ExtractTitle(markdown);
            Assert.Equal("Main Title", result);
        }

        [Fact]
        public void ExtractTitle_WithOnlyH2AndH3_ReturnsH2()
        {
            var markdown = "## Section\n### Subsection\nContent here.";
            var result = MarkdownManager.ExtractTitle(markdown);
            Assert.Equal("Section", result);
        }

        [Fact]
        public void ExtractTitle_WithNoSectionAtAll_ReturnNull()
        {
            var markdown = ".. Section\n... Subsection\nContent here.";
            var result = MarkdownManager.ExtractTitle(markdown);
            Assert.Null(result);
        }

        [Fact]
        public void ExtractFirstHeading_WithOnlyH2_ReturnsH2()
        {
            var markdown = "## Introduction\nSome content.";
            var result = MarkdownManager.ExtractFirstHeading(markdown);
            Assert.Equal("Introduction", result);
        }

        [Fact]
        public void ExtractFirstHeading_WithMultipleHeadings_ReturnsFirstOne()
        {
            var markdown = "## Second\n# First\n### Third";
            var result = MarkdownManager.ExtractFirstHeading(markdown);
            Assert.Equal("Second", result);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Basic_Markdown_Generation_With_CSharpCode()
        {
            var htmlMarkDown = MarkdownManager.ConvertToHtmlFile(BASIC_MARKDOWN_WITH_CSHARP_CODE_1, openInBrowser: true, htmlTemplate: MarkdownManager.HtmlTemplate01);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Basic_Markdown_Generation_1()
        {
            var htmlMarkDown = MarkdownManager.ConvertToHtmlFile(BASIC_MARKDOWN_1, openInBrowser: true, htmlTemplate: MarkdownManager.HtmlTemplate01);
            htmlMarkDown = MarkdownManager.ConvertToHtmlFile(BASIC_MARKDOWN_1, openInBrowser: true, htmlTemplate: MarkdownManager.HtmlTemplate02);
            htmlMarkDown = MarkdownManager.ConvertToHtmlFile(BASIC_MARKDOWN_1, openInBrowser: true, htmlTemplate: MarkdownManager.HtmlTemplate03);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Markdown_LoadWithFrontLoader()
        {
            var MarkDownDocument = MarkdownManager.LoadMarkdownFile(@".\TestFiles\MarkdownWithFrontLoader.2.md");
            var title = "The Ultimate Guide to Markdown Front Matter";
            Assert.Equal(title, MarkDownDocument.FrontMatter.Title);
            Assert.Equal(title, MarkDownDocument.FrontMatter.Name);
            Assert.Equal("An in-depth look at how to use front matter in Markdown files to enhance documentation and project clarity.", MarkDownDocument.FrontMatter.Description);
            Assert.Equal("Jane Doe", MarkDownDocument.FrontMatter.Author);
            Assert.Equal(new DateTime(2023,10,27), MarkDownDocument.FrontMatter.Date);
            Assert.Equal("true", MarkDownDocument.FrontMatter.ExtraFields["published"]);
            Assert.Equal("markdown, guide, metadata", MarkDownDocument.FrontMatter.Tags.Format().Replace(@"""",""));
            Assert.Contains("**Repository URL:** [GitHub repository URL]", MarkDownDocument.MarkdownBody);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Markdown_LoadWithoutBase64Image()
        {
            var MarkDownDocument = MarkdownManager.LoadMarkdownFile(@".\TestFiles\Aristocratic_Desperation_A_Portrait_of_Gambling_Addiction_in_Imperial_Russia_20260529-21.md");
            var md = MarkDownDocument.RawContentWithoutBase64ImageData;

            var expected = @"# Aristocratic Desperation: A Portrait of Gambling Addiction in Imperial Russia

IMAGE 1


IMAGE 2


IMAGE 3


IMAGE 4


## Overall Description";


            Assert.Contains(expected, md);

        }

        [Fact()]
        [TestBeforeAfter]
        public void Markdown_LoadWithFrontLoader_And_Update()
        {
            var markdownFilename = @".\TestFiles\MarkdownWithFrontLoader.md";
            var markDownDocument = MarkdownManager.LoadMarkdownFile(markdownFilename);
            Assert.True(markDownDocument.HasFrontMatter);
            markDownDocument.FrontMatter.Title += " - Updated";
            markDownDocument.FrontMatter.Author += " - Updated";
            markDownDocument.FrontMatter.Description += " - Updated";
            markDownDocument.FrontMatter.Name += " - Updated";
            markDownDocument.FrontMatter.SetDateToNow();

            markDownDocument.MarkdownBody += @"

## Tutu Section
- Tutu chapeau pointu
";
            var markDownDocument2 = MarkdownManager.UpdateMarkdownFile(markdownFilename, markDownDocument.MarkdownBody, markDownDocument.FrontMatter);

            var title = "The Ultimate Guide to Markdown Front Matter";
            Assert.Equal(markDownDocument.FrontMatter.Title, markDownDocument2.FrontMatter.Title);
            Assert.Equal(markDownDocument.FrontMatter.Author, markDownDocument2.FrontMatter.Author);
            Assert.Equal(markDownDocument.FrontMatter.Name, markDownDocument2.FrontMatter.Name);
            Assert.Equal(markDownDocument.FrontMatter.Description, markDownDocument2.FrontMatter.Description);
            Assert.Equal(markDownDocument.FrontMatter.Date, markDownDocument2.FrontMatter.Date);

            Assert.Equal("true", markDownDocument.FrontMatter.ExtraFields["published"]);
            Assert.Equal("markdown, guide, metadata", markDownDocument.FrontMatter.Tags.Format().Replace(@"""", ""));
            Assert.Contains("**Repository URL:** [GitHub repository URL]", markDownDocument.MarkdownBody);
            Assert.Contains("Tutu chapeau pointu", markDownDocument.MarkdownBody);
        }

        [Fact()]
        [TestBeforeAfter]
        public void Markdown_LoadWith_NO_FrontLoader_And_Update()
        {
            var markdownFilename = @".\TestFiles\MarkdownWithNoFrontLoader.md";
            var markDownDocument = MarkdownManager.LoadMarkdownFile(markdownFilename);
            Assert.Null(markDownDocument.FrontMatter);
            Assert.False(markDownDocument.HasFrontMatter);
            markDownDocument.MarkdownBody += @"

## Tutu Section
- Tutu chapeau pointu
";

            var markDownDocument2 = MarkdownManager.UpdateMarkdownFile(markdownFilename, markDownDocument.MarkdownBody, markDownDocument.FrontMatter);
            Assert.Null(markDownDocument2.FrontMatter);

            Assert.Contains("**Repository URL:** [GitHub repository URL]", markDownDocument.MarkdownBody);
            Assert.Contains("Tutu chapeau pointu", markDownDocument.MarkdownBody);
        }

        [Fact()]
        [TestBeforeAfter]
        public void IsMarkdownContentHasFrontLoader_Yes_RemoveIt()
        {
            var mdFile = @".\TestFiles\MarkdownWithFrontLoader.2.md";
            var hasFrontLoader = MarkdownManager.IsMarkdownContentHasFrontLoader(MarkdownManager.LoadMarkdownFile(mdFile).RawContent);
            Assert.True(hasFrontLoader);
            var markDownContent = MarkdownManager.RemoveMarkdownFrontLoader(MarkdownManager.LoadMarkdownFile(mdFile).RawContent).Trim();
            Assert.StartsWith("## 📋 Project Information", markDownContent);
            Assert.EndsWith("quality over quantity!", markDownContent);

            var markDownContent2 = MarkdownManager.RemoveMarkdownFrontLoader(MarkdownManager.LoadMarkdownFile(mdFile).RawContentWithoutBase64ImageData).Trim();
            Assert.StartsWith("## 📋 Project Information", markDownContent2);
            Assert.EndsWith("quality over quantity!", markDownContent2);
        }


        [Fact()]
        [TestBeforeAfter]
        public void IsMarkdownContentHasFrontLoader_No_RemoveIt()
        {
            var mdFile = @".\TestFiles\MarkdownWithNoFrontLoader.md";
            var hasFrontLoader = MarkdownManager.IsMarkdownContentHasFrontLoader(MarkdownManager.LoadMarkdownFile(mdFile).RawContent);
            Assert.False(hasFrontLoader);
            var markDownContent = MarkdownManager.RemoveMarkdownFrontLoader(MarkdownManager.LoadMarkdownFile(mdFile).RawContent).Trim();
            Assert.StartsWith("## 📋 Project Information", markDownContent);
            Assert.EndsWith("quality over quantity!", markDownContent);

            var markDownContent2 = MarkdownManager.RemoveMarkdownFrontLoader(MarkdownManager.LoadMarkdownFile(mdFile).RawContentWithoutBase64ImageData).Trim();
            Assert.StartsWith("## 📋 Project Information", markDownContent2);
            Assert.EndsWith("quality over quantity!", markDownContent2);
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
             MarkdownManager.Clean();
        }
    }
}