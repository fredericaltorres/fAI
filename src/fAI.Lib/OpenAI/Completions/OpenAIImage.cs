using NAudio.SoundFont;
using Newtonsoft.Json;
using System.Diagnostics;

namespace fAI
{
    public partial class OpenAIImage : OpenAIHttpBase
    {
        public OpenAIImage(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut, openAiKey, openAiOrg)
        {
        }

        const string __url = "https://api.openai.com/v1/images/generations";

        public enum OpenAIImageSize  
        { 
            _256x256,
            _512x512,
            _1024x1024,
            _1792x1024,
            _1024x1792
        }

        // https://platform.openai.com/docs/api-reference/images/create
        public ImageResponse Generate(string prompt, string model = "dall-e-3", int imageCount = 1, OpenAIImageSize size = OpenAIImageSize._1024x1024)
        {
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
            else throw new ChatGPTException($"{nameof(Generate)}() failed - {response.Exception.Message}", response.Exception);
        }
    }
}

