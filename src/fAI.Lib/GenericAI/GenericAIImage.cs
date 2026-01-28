using System;
using System.Collections.Generic;

namespace fAI
{
    public partial class GenericAIImage : HttpBase
    {
        public GenericAIImage(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }

        public List<string> GenerateUrl(string prompt, string model = "dall-e-3", int imageCount = 1, OpenAIImage.ImageSize size = OpenAIImage.ImageSize._1024x1024)
        {
            if(model == "dall-e-3")
            {
                var client = new OpenAI(apiKey: base._key);
                var r = client.Image.Generate(prompt, size: size);
                return r.GetUrls();
            }
            else
            {
                throw new Exception($"Model {model} not supported for image generation.");
            }
        }

        public List<string> GenerateLocalFile(string prompt, string model = "dall-e-3", int imageCount = 1, OpenAIImage.ImageSize size = OpenAIImage.ImageSize._1024x1024)
        {
            if (model == "dall-e-3")
            {
                var client = new OpenAI(apiKey: base._key);
                var r = client.Image.Generate(prompt, size: size);
                if(r.Success)
                    return r.DownloadImages();
                else
                    throw new Exception($"Image generation failed: {r.Exception}");
            }
            else
            {
                throw new Exception($"Model {model} not supported for image generation.");
            }
        }
    }
}
