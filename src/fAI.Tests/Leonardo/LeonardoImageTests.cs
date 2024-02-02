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
            var prompt = @"Generate an image inspired by Victor Hugo's classic novel, 'Les Misérables'. 
The image should depict three characters, each with distinct characteristics. 
The first is an older, physically strong man with a scarred face, wearing threadbare clothes, indicative of a hard life — this represents Jean Valjean. 
The second is a young woman radiating innocence and kindness; she wears modest clothes and has beautiful shining eyes — this is Cosette. 
The third is a stern-looking middle-aged man in a gentleman's attire and hat,  representing law and order — representative of Javert. 
Their expressions should reflect the nuances of the complex relationships 
they share in the story.
";
            var client = new Leonardo();
            //var r = client.Image.Generate(prompt, size :  ImageSize._512x512);

            var state = client.Image.GetJobStatus("d18f2d7a-8f25-4735-bbd3-3267861c32ab");
            //var pngFileNames = r.DownloadImage();
            //Assert.True(pngFileNames.Count == 1);
            //Assert.True(File.Exists(pngFileNames[0]));
        }
    }
}