using NAudio.SoundFont;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using static fAI.OpenAIImage;
using System.Linq;
using DynamicSugar;
using static DynamicSugar.DS;
using System.IO;

namespace fAI
{
    public partial class LeonardoImage : HttpBase
    {
        public const string MODEL_ID =  "6bef9f1b-29cb-40c7-b9df-32b51c1f67d3";
        public LeonardoImage(int timeOut = -1) : base(timeOut)
        {
        }

        const string __urlGetUserInfo   = "https://cloud.leonardo.ai/api/rest/v1/me";
        const string __urlGeneations    = "https://cloud.leonardo.ai/api/rest/v1/generations";

        public UserInformation GetUserInformation()
        {
            Trace(new { }, this);

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
            Trace(new { id }, this);

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
            Trace(new { id }, this);

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
            MINIMALIST, CINEMATIC, CREATIVE, VIBRANT, NONE, 
        }

        public enum PresetStyleAlchemyOff
        {
            LEONARDO, NONE
        }

        public enum PresetStyleAlchemyOn
        {
            ANIME, CREATIVE, DYNAMIC, ENVIRONMENT, GENERAL, ILLUSTRATION, PHOTOGRAPHY, RAYTRACED, RENDER_3D, SKETCH_BW, SKETCH_COLOR, NONE
        }

        public enum PromptMagicVersion
        {
            v2, v3
        }

        public string GenerateSync(string prompt,
            string negative_prompt = null,
            string modelId = MODEL_ID,
            StableDiffusionVersion stabeDiffusionVersion = StableDiffusionVersion.v1_5,
            int imageCount = 1,
            ImageSize size = ImageSize._1024x1024,
            bool isPublic = false,
            bool alchemy = true,
            bool photoReal = false,
            bool promptMagic = true,
            PromptMagicVersion promptMagicVersion = PromptMagicVersion.v3,
            int seed = 407795968,
            PresetStyleAlchemyOn presetStyleAlchemyOn = PresetStyleAlchemyOn.DYNAMIC,
            PresetStylePhotoRealOn presetStylePhotoRealOn = PresetStylePhotoRealOn.NONE,
            double promptMagicStrength = 0.5)
        {
            Trace(new { prompt, negative_prompt, modelId, stabeDiffusionVersion, imageCount, size, isPublic, alchemy, photoReal, promptMagic, promptMagicVersion, seed, presetStyleAlchemyOn, presetStylePhotoRealOn, promptMagicStrength }, this);

            GenerationResponse job = Generate(prompt, negative_prompt, modelId, stabeDiffusionVersion, imageCount, size, isPublic, alchemy, photoReal, promptMagic, promptMagicVersion, seed, presetStyleAlchemyOn, presetStylePhotoRealOn, promptMagicStrength);

            var jobState = Managers.TimeOutManager<GenerationResultResponse>("Test", 1, () =>
            {
                var jobS = this.GetJobStatus(job.GenerationId);
                if (jobS.Completed)
                    return jobS;
                return null;
            }, sleepDuration: 6);

            var pngFileNames = jobState.DownloadImages();
            var r = this.DeleteJob(jobState.GenerationId);

            var tfh = new TestFileHelper();
            var newFileName = tfh.GetTempFileName(".jpg");
            File.Move(pngFileNames[0], newFileName);

            return newFileName;
        }

        public enum StableDiffusionVersion
        {
            v1_5,
            v2_1
        }
            // https://docs.leonardo.ai/reference/creategeneration

        public GenerationResponse Generate(string prompt,
            string negative_prompt = null,
            string modelId = MODEL_ID,
            StableDiffusionVersion stabeDiffusionVersion =  StableDiffusionVersion.v1_5,
            int imageCount = 1, 
            ImageSize size = ImageSize._1024x1024,
            bool isPublic = false,
            bool alchemy = true,
            bool photoReal = false,
            bool promptMagic = true,
            PromptMagicVersion promptMagicVersion = PromptMagicVersion.v3,
            int seed = 407795968,
            PresetStyleAlchemyOn presetStyleAlchemyOn = PresetStyleAlchemyOn.DYNAMIC,
            PresetStylePhotoRealOn presetStylePhotoRealOn = PresetStylePhotoRealOn.NONE,
            double promptMagicStrength = 0.5)
        {
            Trace(new { prompt, negative_prompt, modelId, imageCount, size, isPublic, alchemy, photoReal, promptMagic, promptMagicVersion, seed, presetStyleAlchemyOn, presetStylePhotoRealOn, promptMagicStrength }, this);

            var dimensions = size.ToString().Replace("_", "").Split('x').ToList();
            var width = int.Parse(dimensions[0]);
            var height = int.Parse(dimensions[1]);

            var body = new
            {
                prompt,
                negative_prompt,
                modelId = photoReal ? null:modelId,
                sd_version = stabeDiffusionVersion.ToString().Replace("v","").Replace("_","."),
                num_images = imageCount,
                @public = isPublic,
                width,
                height,
                promptMagicStrength,
                alchemy,
                photoReal,
                promptMagic,
                promptMagicVersion = promptMagicVersion.ToString(),
                seed,
                presetStyle = presetStylePhotoRealOn == PresetStylePhotoRealOn.NONE ? presetStyleAlchemyOn.ToString() : presetStylePhotoRealOn.ToString()
            };

            Trace(body, this);

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

