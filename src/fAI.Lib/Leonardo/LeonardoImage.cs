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
    // https://docs.leonardo.ai/docs/api-faq
    // https://docs.leonardo.ai/docs/api-error-messages
    // https://docs.leonardo.ai/docs/elements-and-model-compatibility

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
        public const string LEONARDO_CREATIVE_MODEL_ID =  "6bef9f1b-29cb-40c7-b9df-32b51c1f67d3"; // "Leonardo Creative"
        public LeonardoImage(int timeOut = -1) : base(timeOut)
        {
        }

        const string __urlGetUserInfo   = "https://cloud.leonardo.ai/api/rest/v1/me";
        const string __urlGeneration    = "https://cloud.leonardo.ai/api/rest/v1/generations";
        const string __urlGetGeneationsInfo = "https://cloud.leonardo.ai/api/rest/v1/generations/user/[userId]";
        const string __urlGetElements = "https://cloud.leonardo.ai/api/rest/v1/elements";
        const string __urlGetModels = "https://cloud.leonardo.ai/api/rest/v1/platformModels";


        public GenerationSub GetGenerationsById(string generationId)
        {
            var url = __urlGeneration + $"/{generationId}";
            var response = InitWebClient().GET(url);
            if (response.Success)
                return GenerationsByPk2.FromJson(response.Text);
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

        public List<CustomModel> GetModels(string name = null)
        {
            var url = __urlGetModels;
            var response = InitWebClient().GET(url);
            if (response.Success)
            {
                var rr = Models.FromJson(response.Text);
                
                if (name == null)
                    return rr.custom_models;

                return  rr.custom_models.Where(e => e.name == name).ToList();
            }
            else
                throw LeonardoJsonError.FromJson(response.Text).GetLeonardoException();
        }


        public List<GenerationElement> GetElements(string name = null)
        {
            var url = __urlGetElements;
            var response = InitWebClient().GET(url);
            if (response.Success)
            {
                var rr = GenerationElementRoot.FromJson(response.Text);
                var r = rr.loras;
                if(name == null)
                    return r;
                return r.Where(e => e.name == name).ToList();
            }
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
            ANIME, CREATIVE, DYNAMIC, ENVIRONMENT, GENERAL, ILLUSTRATION, PHOTOGRAPHY, RAYTRACED, RENDER_3D, SKETCH_BW, SKETCH_COLOR, NONE,
            CINEMATIC
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

        // https://docs.leonardo.ai/docs/elements-and-model-compatibility
        public string GenerateSync(string prompt,
            string negative_prompt = null,
            string modelId = null,
            string modelName = null,
            StableDiffusionVersion stableDiffusionVersion = StableDiffusionVersion.v1_5,
            int imageCount = 1,
            ImageSize size = ImageSize._1024x1024,
            bool isPublic = false,
            bool alchemy = true,
            bool photoReal = false,
            float photoRealStrength = 0.5f,
            bool promptMagic = true,
            PromptMagicVersion? promptMagicVersion = PromptMagicVersion.v3,
            int seed = 407795968,
            PresetStyleAlchemyOn presetStyleAlchemyOn = PresetStyleAlchemyOn.DYNAMIC,
            PresetStylePhotoRealOn presetStylePhotoRealOn = PresetStylePhotoRealOn.NONE,
            double? promptMagicStrength = 0.5,
            Scheduler scheduler = Scheduler.LEONARDO, 
            List<GenerationElement> elements = null,
            double elementWeight = 0)
        {
            Trace(new {  modelId, modelName, stableDiffusionVersion, imageCount, size, isPublic, alchemy, photoReal, photoRealStrength, promptMagic, promptMagicVersion, seed, presetStyleAlchemyOn, presetStylePhotoRealOn, promptMagicStrength, scheduler, elements, prompt, negative_prompt, elementWeight }, this);

            GenerationResponse job = GenerateAsync(prompt, negative_prompt, modelId, modelName, stableDiffusionVersion, imageCount, size, isPublic, alchemy, photoReal, photoRealStrength, promptMagic, promptMagicVersion, seed, presetStyleAlchemyOn, presetStylePhotoRealOn, promptMagicStrength, scheduler, elements, elementWeight);

            var jobState = Managers.TimeOutManager<GenerationResultResponse>("Test", 1, () =>
            {
                var jobS = this.GetJobStatus(job.GenerationId);
                return  jobS.Completed ? jobS : null;
            }, sleepDuration: 6);

            var pngFileNames = jobState.DownloadImages();

            var generationJson = this.GetGenerationsById(job.GenerationId);

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

        // https://docs.leonardo.ai/reference/creategeneration
        public GenerationResponse GenerateAsync(string prompt,
            string negative_prompt = null,
            string modelId = null,
            string modelName = null,
            StableDiffusionVersion stableDiffusionVersion =  StableDiffusionVersion.v1_5,
            int imageCount = 1, 
            ImageSize size = ImageSize._1024x1024,
            bool isPublic = false,
            bool alchemy = true,
            bool photoReal = false,
            float photoRealStrength = 0.55f,
            bool promptMagic = true,
            PromptMagicVersion? promptMagicVersion = PromptMagicVersion.v3,
            int seed = 407795968,
            PresetStyleAlchemyOn presetStyleAlchemyOn = PresetStyleAlchemyOn.DYNAMIC,
            PresetStylePhotoRealOn presetStylePhotoRealOn = PresetStylePhotoRealOn.NONE,
            double? promptMagicStrength = 0.5,
            Scheduler scheduler = Scheduler.LEONARDO,
            List<GenerationElement> elements = null,
            double elementWeight = 0
            )
        {
            if (modelName != null)
                modelId = this.GetModels(modelName)[0].id;
            if (modelId == null)
                modelId = LEONARDO_CREATIVE_MODEL_ID;

            Trace(new { modelId, modelName, imageCount, size, isPublic, alchemy, photoReal, promptMagic, promptMagicVersion, seed, presetStyleAlchemyOn, presetStylePhotoRealOn, promptMagicStrength, scheduler, prompt, negative_prompt }, this);

            var dimensions = size.ToString().Replace("_", "").Split('x').ToList();
            var width = int.Parse(dimensions[0]);
            var height = int.Parse(dimensions[1]);

            var presetStyleStr = "NONE";
            if (photoReal)
                presetStyleStr = presetStylePhotoRealOn.ToString();
            else if(alchemy)
                presetStyleStr = presetStyleAlchemyOn.ToString();

            var elements2 = new List<GenerationElementForBody>();
            if (elements != null)
            {
                elements.ForEach(e =>
                {
                    var ee = GenerationElementForBody.FromJson(e);
                    ee.weight = elementWeight;
                    elements2.Add(ee);
                });
            }

            if (elements != null && elements.Count == 0)
                elements2 = null;

            double? nullDouble = null;
            var body = new
            {
                elements = elements2,
                prompt,
                scheduler,
                negative_prompt,
                modelId = photoReal ? null:modelId,
                //modelId,
                sd_version = stableDiffusionVersion.ToString().Replace("v","").Replace("_","."),
                num_images = imageCount,
                @public = isPublic,
                width,
                height,
                promptMagicStrength = promptMagicStrength.HasValue ? promptMagicStrength.Value : nullDouble,
                alchemy,
                photoReal,
                /// photoRealStrength,
                promptMagic,
                promptMagicVersion = promptMagicVersion.HasValue ? promptMagicVersion.ToString() : null as string,
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
                TraceError(erroInfo.ToString(), this);
                throw erroInfo.GetLeonardoException();
            }
        }
    }
}

