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

        public ImageResponse Generate(string prompt, string model = "dall-e-3", int imageCount = 1, string size = "1024x1024")
        {
            var sw = Stopwatch.StartNew();
            var body = new { prompt, model, n=imageCount, size };
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

