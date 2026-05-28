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

        public (string analysis, string title, GenericAICompletions.GenericAIUsage usage) AnalyzeImageFromFile(string model, string imagePath, string prompt = @"
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
                var (analysis, title, usage) = analyzer.AnalyzeImageFromFile(model, imagePath, prompt);
                return (analysis, title, usage);
            }
            else throw new Exception($"Model {model} not supported for image analysis.");
        }

        public (string analysis, string title, GenericAICompletions.GenericAIUsage usage) OcrImageFromFile(string model, string imagePath, string prompt = @"
Perform OCR on this image. Extract all visible text and format the output as valid Markdown.
Use appropriate Markdown elements to reflect the document structure: headings (#, ##, ###)
for titles and section headers, bullet or numbered lists where lists appear, **bold** or
*italic* for emphasized text, `code` or code blocks for any code or monospace content,
tables for tabular data, and blockquotes for quoted content. Preserve the logical hierarchy
and reading order of the original. Output only the Markdown — no preamble, no explanation,
no code fences wrapping the entire output.
")
        {
            if (Anthropic.GetModels().Contains(model))
            {
                var analyzer = new ImageAnalyzer();
                var (extractedMarkdown, title, usage) = analyzer.AnalyzeImageFromFile(model, imagePath, prompt);
                return (extractedMarkdown, title, usage);
            }
            else throw new Exception($"Model {model} not supported for image analysis.");
        }

    }
}