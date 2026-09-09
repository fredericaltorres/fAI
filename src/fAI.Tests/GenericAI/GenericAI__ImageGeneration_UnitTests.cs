using DynamicSugar;
using fAI;
using fAI.OpenAIModel.ImageResponseGpt;
using fAI.Util.Strings;
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
    public class GenericAI__ImageGeneration_UnitTests : OpenAIUnitTestsBase
    {
        
        public GenericAI__ImageGeneration_UnitTests()
        {
            OpenAI.TraceOn = true;
        }



   

        [Fact()]
        [TestBeforeAfter]
        public void GenericAI_Image__futuristic_city_skyline_at_sunset()
        {
            var imagePrompt = @"
A futuristic city skyline at sunset, 
with flying cars and neon lights, 
in the style of cyberpunk, highly detailed, 8k resolution
";
            var client = new GenericAI();
            client.Image.GetModelsApi().ForEach(model =>
            {
                try
                {
                    var imageFileName = Path.Combine(Path.GetTempPath(), $"{ReplaceInvalidFileNameChars(model)}.{Guid.NewGuid()}.jpg");
                    var (image, usage) = client.Image.Create(imagePrompt, model: model, filePath: imageFileName);
                    Assert.True(File.Exists(image));
                    Assert.True(usage.InputTokens > 0);
                    Assert.True(usage.OutputTokens > 0);
                }
                catch (Exception ex)
                {
                    HttpBase.Trace($"[ERROR] Model: {model}, Exception: {ex.Message}", this);
                }
            });
        }


        const string EricClaptonAndEs335Prompt = @"
Create a realistic image based the following content.
IMPORTANT: Do not include any text, letters, numbers, logos, labels, signs, or watermarks anywhere in the image.
------------

MAKE THE IMAGE ONLY ABOUT ERIC CLAPTON PLAYING IN LONDON, 1975, A 1959 RED GIBSON ES 335.

# Gibson ES 335
## Famous Players and the Guitar's Cultural Rise
Famous Players and the Guitar's Cultural Rise
- Chuck Berry: Brought the 335 to mass attention ??? defined the vocabulary of rock and roll guitar.
- Eric Clapton: Used a 1964 ES-335 on the legendary Beano album with John Mayall's Bluesbreakers.
- Freddie King: Searing Texas blues tone ??? articulate but never sterile. A defining sound of the genre.
- Larry Carlton: Nicknamed 'Mr. 335' for his studio work in the 1970s spanning jazz, soul, and fusion.
- Alvin Lee: Blazed through 'I'm Going Home' at Woodstock 1969 ??? one of the festival's great moments. 
- B.B. King: His ES-355 sibling 'Lucille' cemented the semi-hollow as the blues guitar of choice.
------------
";
        [Fact()]
        [TestBeforeAfter]
        public void GenericAI_Image__EricClaptonAndEs335()
        {
            var client = new GenericAI();
            client.Image.GetCheapModels().ForEach(model =>
            {
                try
                {
                    var imageFileName = Path.Combine(Path.GetTempPath(), $"{ReplaceInvalidFileNameChars(model)}.{Guid.NewGuid()}.jpg");
                    var (image, usage) = client.Image.Create(EricClaptonAndEs335Prompt, model: model, filePath: imageFileName);
                    Assert.True(File.Exists(image));
                    Assert.True(usage.InputTokens > 0);
                    Assert.True(usage.OutputTokens > 0);
                }
                catch (Exception ex)
                {
                    HttpBase.Trace($"[ERROR] Model: {model}, Exception: {ex.Message}", this);
                }
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAI_Image__NanoBanana__EricClaptonAndEs335()
        {
            var client = new GenericAI();
            client.Image.GetNanoBananaModels().ForEach(model =>
            {
                try
                {
                    var imageFileName = Path.Combine(Path.GetTempPath(), $"{ReplaceInvalidFileNameChars(model)}.{Guid.NewGuid()}.jpg");
                    var (image, usage) = client.Image.Create(EricClaptonAndEs335Prompt, model: model, filePath: imageFileName);
                    Assert.True(File.Exists(image));
                    Assert.True(usage.InputTokens > 0);
                    Assert.True(usage.OutputTokens > 0);
                }
                catch (Exception ex)
                {
                    HttpBase.Trace($"[ERROR] Model: {model}, Exception: {ex.Message}", this);
                }
            });
        }

        [Fact()]
        [TestBeforeAfter]
        public void GenericAI_Image__openai_gpt_image_2_5_sunburst__EricClaptonAndEs335()
        {
            var client = new GenericAI();
            var model = "openai/gpt-image-2.5-sunburst";
            try
            {
                var imageFileName = Path.Combine(Path.GetTempPath(), $"{ReplaceInvalidFileNameChars(model)}.{Guid.NewGuid()}.jpg");
                var (image, usage) = client.Image.Create(EricClaptonAndEs335Prompt, model: model, filePath: imageFileName);
                Assert.True(File.Exists(image));
                Assert.True(usage.InputTokens > 0);
                Assert.True(usage.OutputTokens > 0);
            }
            catch (Exception ex)
            {
                HttpBase.Trace($"[ERROR] Model: {model}, Exception: {ex.Message}", this);
            }
        }

    }
}