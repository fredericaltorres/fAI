using DynamicSugar;
using fAI.OpenAIModel.ImageResponseGpt;
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

        const string __url_dalle = "https://api.openai.com/v1/images/generations";
        const string __url_gpt = "https://api.openai.com/v1/responses";
        

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

            var tfh = new TestFileHelper();
            var newImage = tfh.GetTempFileName(".png");

            var body = new
            {
                model = model,
                input = prompt,
                tools = new[]
                {
                    new { type = "image_generation",  action = "generate" }
                }
            };

            var response = InitWebClient().POST(__url_gpt, JsonConvert.SerializeObject(body));
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                Logger.Trace($"{response.Text}", this);
                var imageResponse = OpenAIGpt55ImageResponse.FromJson(response.Text);
                r._downloadImages = imageResponse.GetLocalImages();
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
            var response = InitWebClient().POST(__url_dalle, JsonConvert.SerializeObject(body));
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

