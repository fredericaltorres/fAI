using NAudio.SoundFont;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using static fAI.OpenAIImage;
using System.Linq;

namespace fAI
{
    public partial class LeonardoImage : HttpBase
    {
        public const string MODEL_ID =  "6bef9f1b-29cb-40c7-b9df-32b51c1f67d3";
        public LeonardoImage(int timeOut = -1) : base(timeOut)
        {
        }

        const string __urlGetUserInfo = "https://cloud.leonardo.ai/api/rest/v1/me";
        const string __urlGeneations = "https://cloud.leonardo.ai/api/rest/v1/generations";
                                        

        public UserInformation GetUserInformation()
        {
            OpenAI.Trace(new { }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().GET(__urlGetUserInfo);
            sw.Stop();
            if (response.Success)
            {
                var r = UserInformation.FromJson(response.Text);
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                return null;
            }
        }

        public GenerationResultResponse GetJobStatus(string id)
        {
            OpenAI.Trace(new { }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().GET($"{__urlGeneations}/{id}");
            sw.Stop();
            if (response.Success)
            {
                var r = GenerationResultResponse.FromJson(response.Text);
                r.Stopwatch = sw;
                return r;
            }
            else return new GenerationResultResponse {  Exception = response.Exception };
        }

        // https://docs.leonardo.ai/reference/creategeneration
        public GenerationResponse Generate(string prompt, 
            string negative_prompt = null,
            string modelId = MODEL_ID, 
            int imageCount = 1, 
            ImageSize size = ImageSize._1024x1024,
            bool isPublic = false)
        {
            var dimensions = size.ToString().Replace("_", "").Split('x').ToList();
            var width = int.Parse(dimensions[0]);
            var height = int.Parse(dimensions[1]);

            var body = new
            {
                prompt,
                negative_prompt,
                modelId,
                size,
                num_images = imageCount,
                @public = isPublic,
                width,
                height
            };

            OpenAI.Trace(body, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(__urlGeneations, JsonConvert.SerializeObject(body));
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = GenerationResponse.FromJson(response.Text);
                r.Stopwatch = sw;
                return r;
            }
            else throw new ChatGPTException($"{response.Exception.Message}", response.Exception);
        }
    }
}

