using AnthropicImageAnalysis;
using System;
using System.Collections.Generic;
using static fAI.HumeAISpeech;

namespace fAI
{

    public partial class GenericAIImage : HttpBase
    {
        public GenericAIImage(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }

        public List<string> GenerateUrl(string prompt, string model = "dall-e-3", int imageCount = 1, OpenAIImage.ImageSize size = OpenAIImage.ImageSize._1024x1024)
        {
            if (model == "dall-e-3")
            {
                var client = new OpenAI(apiKey: base._key);
                var r = client.Image.GenerateDalle(prompt, size: size);
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
                var r = client.Image.GenerateDalle(prompt, size: size, model: model);
                if (r.Success)
                    return r.DownloadImages();
                else
                    throw new Exception($"Image generation failed: {r.Exception}");
            }
            else if (model == "gpt-5.5" || model == "gpt-5.3" /* fast one*/)
            {
                var client = new OpenAI(apiKey: base._key);
                var r = client.Image.GenerateGpt(prompt, size: size, model: model);
                if (r.Success)
                    return r.DownloadImages();
                else
                    throw new Exception($"Image generation failed: {r.Exception}");
            }
            else
            {
                throw new Exception($"Model {model} not supported for image generation.");
            }
        }

        public string AnalyzeImageFromFile(string model, string imagePath, string prompt = @"
Please analyze this image thoroughly and provide:
1. **Overall Description** - A concise summary of what the image shows.
2. **Key Elements** - List the main subjects, objects, or focal points.
3. **Colors & Composition** - Describe the dominant colors, lighting, and layout.
4. **Context & Setting** - Infer the environment, time of day, or scenario if possible.
5. **Mood & Atmosphere** - Describe the emotional tone or feeling conveyed.
6. **Notable Details** - Highlight any interesting, unusual, or fine-grained details.
Be specific and descriptive in your analysis.

Use MARKDOWN syntax for formatting the response, with headings and bullet points where appropriate.
")
        {
            if (Anthropic.GetModels().Contains(model))
            {
                var analyzer = new ImageAnalyzer();
                var analysis = analyzer.AnalyzeImageFromFile(imagePath, prompt);
                return analysis;
            }
            else throw new Exception($"Model {model} not supported for image analysis.");
        }
    }
}