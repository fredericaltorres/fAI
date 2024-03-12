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
using Newtonsoft.Json.Converters;
using static fAI.LeonardoGeneration;

namespace fAI
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    //public class LeonardoWebError
    //{
    //    public string error { get; set; }
    //    public string path { get; set; }
    //    public string code { get; set; }

    //    public static LeonardoWebError FromJSON(string json)
    //    {
    //        return JsonConvert.DeserializeObject<LeonardoWebError>(json);
    //    }
    //}



    public partial class LeonardoImage : HttpBase
    {
        public const string MODEL_ID =  "6bef9f1b-29cb-40c7-b9df-32b51c1f67d3";
        public LeonardoImage(int timeOut = -1) : base(timeOut)
        {
        }

        const string __urlGetUserInfo   = "https://cloud.leonardo.ai/api/rest/v1/me";
        const string __urlGeneration    = "https://cloud.leonardo.ai/api/rest/v1/generations";
        const string __urlGetGeneationsInfo = "https://cloud.leonardo.ai/api/rest/v1/generations/user/[userId]";


        public Generation GetGenerationsById(string generationId)
        {
            var url = __urlGeneration + $"/{generationId}";
            var response = InitWebClient().GET(url);
            if (response.Success)
                return Generation.FromJson(response.Text);
            else
                throw LeonardoJsonError.FromJson(response.Text).GetLeonardoException();
        }

        public LeonardoGeneration GetGenerationsByUserId(string userId, int offSet = 0, int limit = 100)
        {
            if(userId == null)
            {
                var userInfo = this.GetUserInformation();
                userId = userInfo.user_details[0].user.id;
            }

            var url = (__urlGetGeneationsInfo + $"?offset={offSet}&limit={limit}").Template(new { userId }, "[", "]");
            var response = InitWebClient().GET(url);
            if(response.Success)
                return LeonardoGeneration.FromJson(response.Text);
            else
                throw LeonardoJsonError.FromJson(response.Text).GetLeonardoException();
        }

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
            var response = InitWebClient().DELETE($"{__urlGeneration}/{id}");
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
            var response = InitWebClient().GET($"{__urlGeneration}/{id}");
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
            MINIMALIST, CINEMATIC, CREATIVE, VIBRANT, NONE, LEONARDO
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


        [JsonConverter(typeof(StringEnumConverter))]
        public enum Scheduler
        {
            LEONARDO
        }

        public string GenerateSync(string prompt,
            string negative_prompt = null,
            string modelId = MODEL_ID,
            StableDiffusionVersion stableDiffusionVersion = StableDiffusionVersion.v1_5,
            int imageCount = 1,
            ImageSize size = ImageSize._1024x1024,
            bool isPublic = false,
            bool alchemy = true,
            bool photoReal = false,
            float photoRealStrength = 0.55f,
            bool promptMagic = true,
            PromptMagicVersion promptMagicVersion = PromptMagicVersion.v3,
            int seed = 407795968,
            PresetStyleAlchemyOn presetStyleAlchemyOn = PresetStyleAlchemyOn.DYNAMIC,
            PresetStylePhotoRealOn presetStylePhotoRealOn = PresetStylePhotoRealOn.NONE,
            double promptMagicStrength = 0.5,
            Scheduler scheduler = Scheduler.LEONARDO, 
            List<GenerationElement> elements = null)
        {
            Trace(new { prompt, negative_prompt, modelId, stableDiffusionVersion, imageCount, size, isPublic, alchemy, photoReal, photoRealStrength, promptMagic, promptMagicVersion, seed, presetStyleAlchemyOn, presetStylePhotoRealOn, promptMagicStrength, scheduler, elements }, this);

            GenerationResponse job = GenerateSync2(prompt, negative_prompt, modelId, stableDiffusionVersion, imageCount, size, isPublic, alchemy, photoReal, photoRealStrength, promptMagic, promptMagicVersion, seed, presetStyleAlchemyOn, presetStylePhotoRealOn, promptMagicStrength, scheduler, elements);

            var jobState = Managers.TimeOutManager<GenerationResultResponse>("Test", 1, () =>
            {
                var jobS = this.GetJobStatus(job.GenerationId);
                return  jobS.Completed ? jobS : null;
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
            v0_9,
            v1_5,
            v2_1,
            v3
        }
        // https://docs.leonardo.ai/reference/creategeneration

        public List<string> WaitForImages(GenerationResponse requestJob, int timeOut = 1)
        {
            var r = new List<string>();
            GenerationResultResponse jobState = null;
            var jobSucceeded = Managers.TimeOutManager("Test", timeOut, () =>
            {
                jobState = this.GetJobStatus(requestJob.GenerationId);
                return jobState.Completed;
            });
            if (jobSucceeded)
                r = jobState.DownloadImages();
            else 
                throw new LeonardoException($"Image generation did not finish in {timeOut} minutes");

            if(jobState != null)
                this.DeleteJob(jobState.GenerationId);

            return r; 
        }

        public GenerationResponse Generate(string jsonBody)
        {
            Trace(jsonBody, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(__urlGeneration, jsonBody);
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = GenerationResponse.FromJson(response.Text);
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                var erroInfo = LeonardoJsonError.FromJson(response.Text);
                throw erroInfo.GetLeonardoException();
            }
        }

        public GenerationResponse GenerateSync2(string prompt,
            string negative_prompt = null,
            string modelId = MODEL_ID,
            StableDiffusionVersion stableDiffusionVersion =  StableDiffusionVersion.v1_5,
            int imageCount = 1, 
            ImageSize size = ImageSize._1024x1024,
            bool isPublic = false,
            bool alchemy = true,
            bool photoReal = false,
            float photoRealStrength = 0.55f,
            bool promptMagic = true,
            PromptMagicVersion promptMagicVersion = PromptMagicVersion.v3,
            int seed = 407795968,
            PresetStyleAlchemyOn presetStyleAlchemyOn = PresetStyleAlchemyOn.DYNAMIC,
            PresetStylePhotoRealOn presetStylePhotoRealOn = PresetStylePhotoRealOn.NONE,
            double promptMagicStrength = 0.5,
            Scheduler scheduler = Scheduler.LEONARDO,
            List<GenerationElement> elements = null
            )
        {
            Trace(new { prompt, negative_prompt, modelId, imageCount, size, isPublic, alchemy, photoReal, promptMagic, promptMagicVersion, seed, presetStyleAlchemyOn, presetStylePhotoRealOn, promptMagicStrength, scheduler }, this);

            var dimensions = size.ToString().Replace("_", "").Split('x').ToList();
            var width = int.Parse(dimensions[0]);
            var height = int.Parse(dimensions[1]);

            var presetStyleStr = "NONE";
            if (photoReal)
                presetStyleStr = presetStylePhotoRealOn.ToString();
            else if(alchemy)
                presetStyleStr = presetStyleAlchemyOn.ToString();

            var body = new
            {
                elements,
                prompt,
                scheduler,
                negative_prompt,
                modelId = photoReal ? null:modelId,
                sd_version = stableDiffusionVersion.ToString().Replace("v","").Replace("_","."),
                num_images = imageCount,
                @public = isPublic,
                width,
                height,
                promptMagicStrength,
                alchemy,
                photoReal,
                /// photoRealStrength,
                promptMagic,
                promptMagicVersion = promptMagicVersion.ToString(),
                seed,
                presetStyle = presetStyleStr,
            };

            Trace(body, this);
            var jsonBody = JsonConvert.SerializeObject(body);
            Trace($"Body.Json:{jsonBody}", this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(__urlGeneration, jsonBody);
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = GenerationResponse.FromJson(response.Text);
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                var erroInfo = LeonardoJsonError.FromJson(response.Text);
                throw erroInfo.GetLeonardoException();
            }
        }
    }
}

