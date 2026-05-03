using NAudio.SoundFont;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text.Json;

namespace fAI
{
    public partial class OpenAIImage : HttpBase
    {
        public OpenAIImage(int timeOut = -1, string openAiKey = null) : base(timeOut, openAiKey)
        {
        }

        const string __url = "https://api.openai.com/v1/images/generations";

        public enum ImageSize  
        { 
            _256x256,
            _512x512,
            _576x1024,
            _512x984,
            _768x1360,
            _1024x1024,
            _1024x768,
            _1024x576,
            _1792x1024,
            _1024x1792,
            _1360x768,
        }

        public ImageResponse GenerateGpt(string prompt, string model = "gpt-5.5", int imageCount = 1, ImageSize size = ImageSize._1024x1024)
        {
            OpenAI.Trace(new { prompt, model, size }, this);
            var r = new ImageResponse();
            var sw = Stopwatch.StartNew();

            var body = new
            {
                model = model,
                input = prompt,
                tools = new[]
                {
                    new { type = "image_generation",  action = "generate" }
                }
            };

            var response = InitWebClient().POST(__url, JsonConvert.SerializeObject(body));
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                using (var doc = JsonDocument.Parse(response.Text)) 
                { 
                    if (doc.RootElement.TryGetProperty("output", out var outputArray))
                    {
                        foreach (var item in outputArray.EnumerateArray())
                        {
                            if (item.TryGetProperty("content", out var contentArray))
                            {
                                foreach (var contentItem in contentArray.EnumerateArray())
                                {
                                    if (contentItem.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "output_image")
                                    {
                                        var base64 = contentItem.GetProperty("image_base64").GetString();
                                        var bytes = Convert.FromBase64String(base64);
                                        System.IO.File.WriteAllBytes("cat_otter.png", bytes);
                                    }
                                }
                            }
                        }
                    }
                }
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                return new ImageResponse
                {
                    Exception = new ChatGPTException($"{response.Exception.Message}", response.Exception)
                };
            }
        }

        // https://platform.openai.com/docs/api-reference/images/create
        public ImageResponse GenerateDalle(string prompt, string model = "dall-e-3", int imageCount = 1, ImageSize size = ImageSize._1024x1024)
        {
            OpenAI.Trace(new { prompt, model, size }, this);

            var sw = Stopwatch.StartNew();
            var body = new { prompt, model, n=imageCount, size= size.ToString().Replace("_","") };
            var response = InitWebClient().POST(__url, JsonConvert.SerializeObject(body));
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = ImageResponse.FromJson(response.Text);
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                return new ImageResponse
                {
                    Exception = new ChatGPTException($"{response.Exception.Message}", response.Exception)
                };
            }
        }
    }
}

