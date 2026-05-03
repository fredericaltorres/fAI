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
    }
}