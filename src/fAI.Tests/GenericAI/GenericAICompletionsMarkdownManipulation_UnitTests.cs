using DynamicSugar;
using fAI;
using Markdig;
using Newtonsoft.Json;
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
    public class GenericAICompletionsMarkdownManipulation_UnitTests : OpenAIUnitTestsBase
    {
        public GenericAICompletionsMarkdownManipulation_UnitTests()
        {
            OpenAI.TraceOn = true;
        }


        const string markDownContentTestPlan1 = @"
# Test Plans

# Module Test Plans

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
| 13  DeliveryMonitor (EmailDeliveryHandler) |
";

        [Fact()]
        [TestBeforeAfter]
        public void Conversation_GenericAI_MarkDownManipulation()
        {
            var instruction = "mark test plan 2 as done";

            var markdownContent = markDownContentTestPlan1;

            var MarkDownEditorSystemPrompt = $@"
                    You are a markdown editor.
                    Your job is to apply the requested changes to the markdown content and return ONLY the updated markdown 
                    with no explanation or commentary.

                    When marking an item as ""done"", add a ✅ emoji at the end of that row.
                    When marking an item with some ""issue"", add a ❌ emoji at the end of that row.
                    When marking an item with ""warning"", add a ⚠️ emoji at the end of that row.

                    Return ONLY the updated markdown content.
            ";

            var prompt = $@"

                    Instruction: {instruction}.

                    Markdown Content:
                    {markdownContent}
            ";

            var models = DS.List("gemini-2.0-flash", "claude-haiku-4-5"); // , "claude-sonnet-4-5", , "gpt-5-mini"
            foreach (var model in models)
            {
                var sw = Stopwatch.StartNew();
                var client = new GenericAI();
                var (text, contents) = client.Completions.Create(prompt, model: model, systemPrompt: MarkDownEditorSystemPrompt);
                sw.Stop();
                var htmlMarkDown = MarkdownManager.ConvertToHtmlFile(text, true);
                HttpBase.Trace($"[CONVERSATION] Model: {model}, Duration: {sw.ElapsedMilliseconds / 1000.0:0.0}, Response: {text}", this);
            }
        }

        [Fact()]
        [TestBeforeAfter]
        public void Markdown_Generation()
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
    }
}