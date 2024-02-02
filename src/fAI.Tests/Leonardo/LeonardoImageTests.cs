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
        }

        [Fact()]
        public void Image_Generate()
        {
            var prompt = @"
Portrait of a undead Kid goblin girl doing martial arts, cyborg parts, futuristic city, neon signs, Japanese city, 
in a Cyberpunk city alley, creative_techwear_clothes, warhammer_chaos_marine, art by Mika Pikazo, guweiz, 
Wlop, unique detail, masterpiece
";
            var negativePrompt = @"Close up face, EasyNegative, (badv2:0.8), (badhandv4:1.18), (bad quality:1.3), (worst quality:1.3), watermark, (blurry), (cropped), (cleavage:1.3) canvas frame, cartoon, 3d, , ((bad art)), extra limbs)),((close up)),((b&w)), , signature, blurry, (((duplicate))), ((morbid)), ((mutilated)), [out of frame], extra fingers, mutated hands, ((poorly drawn hands)), ((poorly drawn face)), blurry,, ((extra limbs)), cloned face, out of frame, ugly, extra limbs, (malformed limbs), ((missing arms)), ((missing legs)), (((extra arms))), (((extra legs))), mutated hands, (fused fingers), (too many fingers), (((long neck))), tiling, poorly drawn hands, poorly drawn feet, poorly drawn face, out of frame, extra limbs, extra legs, extra arms, cross-eye, body out of frame, blurr";

            var client = new Leonardo();
            var job = client.Image.Generate(
                prompt, 
                negative_prompt: negativePrompt,
                size : ImageSize._1024x1024);

            var jobState = client.Image.GetJobStatus(job.GenerationId);

            var pngFileNames = jobState.DownloadImages();
            Assert.True(pngFileNames.Count == 1);
            Assert.True(File.Exists(pngFileNames[0]));
        }
    }
}