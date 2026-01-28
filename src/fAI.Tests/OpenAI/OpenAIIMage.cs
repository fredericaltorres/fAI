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
    public class OpenAIImageTests
    {
        const string prompt = @"Generate an image inspired by Victor Hugo's classic novel, 'Les Misérables'. 
The image should depict three characters, each with distinct characteristics. 
The first is an older, physically strong WHITE man with a scarred face, wearing threadbare clothes, indicative of a hard life — this represents Jean Valjean. 
The second is a young WHITE woman radiating innocence and kindness; she wears modest clothes and has beautiful shining eyes — this is Cosette. 
The third is a stern-looking middle-aged WHITE, EUROPEAN man in a gentleman's attire and hat,  representing law and order — representative of Javert. 
Their expressions should reflect the nuances of the complex relationships 
they share in the story.
";

        [Fact()]
        public void Image_Generate()
        {
           
            var client = new OpenAI();
            var r = client.Image.Generate(prompt, size :  ImageSize._1792x1024);
            var pngFileNames = r.DownloadImages();
            Assert.True(pngFileNames.Count == 1);
            Assert.True(File.Exists(pngFileNames[0]));
        }

        [Fact()]
        public void GenericAI_Image_Generate()
        {
            var client = new GenericAI();
            //var urls = client.Images.GenerateUrl(prompt, size: ImageSize._1024x1024);
            var files = client.Images.GenerateLocalFile(prompt, size: ImageSize._1024x1024);
        }
    }
}