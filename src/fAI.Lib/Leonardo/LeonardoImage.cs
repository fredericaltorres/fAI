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

        public DeleteGenerationsResponse DeleteJob(string id)
        {
            OpenAI.Trace(new { }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().DELETE($"{__urlGeneations}/{id}");
            sw.Stop();
            if (response.Success)
            {
                var r = DeleteGenerationsResponse.FromJson(response.Text);
                r.Stopwatch = sw;
                return r;
            }
            else return new DeleteGenerationsResponse { Exception = response.Exception };
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

        public enum PresetStylePhotoRealOn
        {
            CINEMATIC, CREATIVE, VIBRANT, NONE
        }

        public enum PresetStyleAlchemyOff
        {
            LEONARDO, NONE
        }

        public enum PresetStyleAlchemyOn
        {
            ANIME, CREATIVE, DYNAMIC, ENVIRONMENT, GENERAL, ILLUSTRATION, PHOTOGRAPHY, RAYTRACED, RENDER_3D, SKETCH_BW, SKETCH_COLOR, NONE
        }

        public enum PromptMagic
        {
            V2, V3
        }

        // https://docs.leonardo.ai/reference/creategeneration
        public GenerationResponse Generate(string prompt, 
            string negative_prompt = null,
            string modelId = MODEL_ID, 
            int imageCount = 1, 
            ImageSize size = ImageSize._1024x1024,
            bool isPublic = false,
            bool alchemy = true,
            PromptMagic promptMagic = PromptMagic.V3,
            int seed = 407795968,
            PresetStyleAlchemyOn presetStyleAlchemyOn = PresetStyleAlchemyOn.DYNAMIC,
            double promptMagicStrength = 0.5)
        {
            var dimensions = size.ToString().Replace("_", "").Split('x').ToList();
            var width = int.Parse(dimensions[0]);
            var height = int.Parse(dimensions[1]);

            var body = new
            {
                prompt,
                negative_prompt,
                modelId,
                num_images = imageCount,
                @public = isPublic,
                width,
                height,
                promptMagicStrength,
                alchemy,
                promptMagic = promptMagic.ToString(),
                seed,
                presetStyle = presetStyleAlchemyOn.ToString()
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

