using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.OpenAICompletions;
using System.Runtime.InteropServices;
using static fAI.OpenAIImage;
using DynamicSugar;
using System.Threading;
using static fAI.LeonardoImage;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class LeonardoImageTests
    {
        [Fact()]
        public void GetUserInformation()
        {
            var client = new Leonardo();
            var userInfo = client.Image.GetUserInformation();
            Assert.StartsWith("frederic", userInfo.user_details[0].user.username);
            Assert.True(userInfo.user_details[0].subscriptionTokens > 1000);
            Assert.True(userInfo.user_details[0].subscriptionGptTokens > 100);
            Assert.True(userInfo.user_details[0].subscriptionModelTokens > 1);
            Assert.True(userInfo.user_details[0].apiConcurrencySlots > 1);

            var userId = userInfo.user_details[0].user.id;

            var generations = client.Image.GetGenerationsByUserId(userInfo.user_details[0].user.id);
            var pocoPrompt = generations.generations[0].GetPromptParametersInPocoFormat();
        }

        [Fact()]
        public void Image_Generate_1()
        {
            var prompt = @"
Portrait of a undead Kid goblin girl doing martial arts, cyborg parts, futuristic city, neon signs, Japanese city, 
in a Cyberpunk city alley, creative_techwear_clothes, warhammer_chaos_marine, art by Mika Pikazo, guweiz, 
Wlop, unique detail, masterpiece
";
            var negativePrompt = @"Close up face, EasyNegative, (badv2:0.8), (badhandv4:1.18), (bad quality:1.3), (worst quality:1.3), watermark, (blurry), (cropped), (cleavage:1.3) canvas frame, cartoon, 3d, , ((bad art)), extra limbs)),((close up)),((b&w)), , signature, blurry, (((duplicate))), ((morbid)), ((mutilated)), [out of frame], extra fingers, mutated hands, ((poorly drawn hands)), ((poorly drawn face)), blurry,, ((extra limbs)), cloned face, out of frame, ugly, extra limbs, (malformed limbs), ((missing arms)), ((missing legs)), (((extra arms))), (((extra legs))), mutated hands, (fused fingers), (too many fingers), (((long neck))), tiling, poorly drawn hands, poorly drawn feet, poorly drawn face, out of frame, extra limbs, extra legs, extra arms, cross-eye, body out of frame, blurr";

            var client = new Leonardo();
            var job = client.Image.Generate(
                prompt, negative_prompt: negativePrompt, size: ImageSize._1024x1024
                ,seed : 407795969
                );

            var pngFileNames = client.Image.WaitForImages(job);
            pngFileNames.ForEach((f) => Assert.True(File.Exists(f)));
            pngFileNames.ForEach((f) => File.Delete(f));
        }

        [Fact()]
        public void Image_Generate_photoReal_on()
        {
            var prompt = @"
high quality, 8K Ultra HD, In this extraordinary full-body digital illustration, envision the enchanting presence of a captivating woman with ethereal features. Her elegant allure takes center stage, accentuated by flowing golden locks and mesmerizing cyan eyes, creating an otherworldly charm that captivates the observer, However, in this arrangement, her gaze holds a subtle hint of intrigue, inviting viewers to delve into the mystery that surrounds her, As a masterful touch, consider adding a celestial glow to her entire being, emanating softly from the stars above and reflecting in her eyes, by yukisakura, awesome full color,
";
            var negativePrompt = @"";

            var client = new Leonardo();
            var job = client.Image.Generate(
                prompt, 
                //negative_prompt: negativePrompt, 
                size: ImageSize._512x984,
                seed: 276570112, 
                photoReal: true,
                presetStylePhotoRealOn: PresetStylePhotoRealOn.MINIMALIST
            );

            var pngFileNames = client.Image.WaitForImages(job);

            Assert.True(pngFileNames.Count == 1);
            Assert.True(File.Exists(pngFileNames[0]));
            File.Delete(pngFileNames[0]);
        }

        [Fact()]
        public void Image_Generate_WomanAtDifferentAge()
        {
            var prompt = @"
Close-up portrait BLONDE WOMAN [AGE] years old, nice body shape,|(STYLED HAIR:1.7), color portrait, Linkedin profile picture, professional portrait photography by Martin Schoeller, by Mark Mann, by Steve McCurry, bokeh, studio lighting, canonical lens, shot on dslr, 64 megapixels, sharp focus.
";
            var ages = new List<int>() { 60 };

            var finalOutputFiles = new FileSequenceManager(@"c:\temp\@fAiImages");

            foreach (var age in ages)
            {
                var client = new Leonardo();

                var fileName = client.Image.GenerateSync(prompt.Replace("[AGE]", age.ToString()), 
                                size: ImageSize._768x1360, seed: 689242880);

                finalOutputFiles.AddFile(fileName, move: true);
            }
        }
    }
}