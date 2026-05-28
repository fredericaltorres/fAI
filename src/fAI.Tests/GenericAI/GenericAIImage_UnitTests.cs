using DynamicSugar;
using fAI;
using Markdig;
using Markdig.Syntax.Inlines;
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
    public class GenericAIImage_UnitTests : OpenAIUnitTestsBase
    {
        public GenericAIImage_UnitTests()
        {
            OpenAI.TraceOn = true;
        }

        const string imagePrompt = "Generate an image of gay tabby cat hugging a woman with an orange scarf, in London 1875.";
        [Fact()]
        [TestBeforeAfter]
        public void GenerateImage()
        {
            var i = new GenericAIImage();
            DS.List("gpt-5.5", "dall-e-3" ).ForEach(model => 
            {
                var images = i.GenerateLocalFile(imagePrompt, model);
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void Completion_AnalyzeImage()
        {
            var imageFileName = base.GetTestFile("ManAndBoartInStorm.png");
            var i = new GenericAIImage();
            Anthropic.GetModels().Take(1).ToList().ForEach(model =>
            {
                var (description, title, usage) = i.AnalyzeImageFromFile(model, imageFileName);
                Assert.True(description.Length > 0);
                Assert.True(title.Length > 0);    
                Assert.NotNull(usage);
                Assert.True(usage.InputTokens > 0);
                Assert.True(usage.OutputTokens > 0);
                Assert.True(usage.Duration > 0);
            });
        }


        [Fact()]
        [TestBeforeAfter]
        public void Completion_OCR()
        {
            var imageFileName = base.GetTestFile("OCR_1.png");
            var i = new GenericAIImage();
            Anthropic.GetModels().Take(1).ToList().ForEach(model =>
            {
                var (markdownExtracted, title, usage) = i.OcrImageFromFile(model, imageFileName);
                Assert.True(markdownExtracted.Length > 0);
                Assert.True(title.Length > 0);
                Assert.NotNull(usage);
                Assert.True(usage.InputTokens > 0);
                Assert.True(usage.OutputTokens > 0);
                Assert.True(usage.Duration > 0);
            });
        }
    }
}