using DynamicSugar;
using fAI.Util.Strings;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static fAI.GenericAIUtility;
using static System.Net.WebRequestMethods;

namespace fAI
{
    public class GenericAIImage : HttpBase
    {
        //https://openrouter.ai/docs/api/api-reference/images/generate-an-image
        public const string __url = "https://openrouter.ai/api/v1/images";

        public GenericAIImage(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Architecture
        {
            public List<string> input_modalities { get; set; }
            public List<string> output_modalities { get; set; }
        }

        public class DatumModelWbApi
        {
            public Architecture architecture { get; set; }
            public int created { get; set; }
            public string description { get; set; }
            public string endpoints { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public SupportedParameters supported_parameters { get; set; }
            public bool supports_streaming { get; set; }
        }

        public class Resolution
        {
            public string type { get; set; }
            public List<string> values { get; set; }
        }

        public class GetModelsResponse
        {
            public List<DatumModelWbApi> data { get; set; }

            public static GetModelsResponse FromJson(string json) => JsonConvert.DeserializeObject<GetModelsResponse>(json, new IsoDateTimeConverter { DateTimeStyles = System.Globalization.DateTimeStyles.AssumeUniversal });
        }

        public class SupportedParameters
        {
            public Resolution resolution { get; set; }
        }

        public List<string> GetModelsApi()
        {
            OpenAI.Trace(new { }, this);

            if (base._key == null)
                base._key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

            var wc = InitWebClient();
            var response = wc.GET("https://openrouter.ai/api/v1/images/models");
            if (response.Success)
            {
                var r = GetModelsResponse.FromJson(response.Text);
                Logger.Trace(response.Text, this);
                return r.data.Select(x => x.id).ToList();
            }
            else throw new OpenAIAudioSpeechException($"{nameof(GetModelsApi)}() failed - {response.Exception.Message}", response.Exception);
        }

        public List<string> GetCheapModels()
        {
            return DS.List(
                "meta/muse-image",
                "bytedance-seed/seedream-5-0-lite",
                "qwen/qwen-image-3",
                "krea/krea-2-medium",
                "krea/krea-2-medium-turbo",
                "google/gemini-3.1-flash-lite-image",
                "openai/gpt-image-2",
                "openai/gpt-image-1-mini",
                "sourceful/riverflow-v2.5-fast",
                "microsoft/mai-image-2.5",
                "x-ai/grok-imagine-image-quality",
                "recraft/recraft-v4.1-utility",
                "recraft/recraft-v4.1",
                "recraft/recraft-v4",
                "recraft/recraft-v3",
                "openai/gpt-5.4-image-2",
                "sourceful/riverflow-v2-fast",
                "black-forest-labs/flux.2-klein-4b",
                "bytedance-seed/seedream-4.5",
                "black-forest-labs/flux.2-flex",
                "black-forest-labs/flux.2-pro",
                "openai/gpt-5-image-mini",
                "google/gemini-2.5-flash-image"
                );
        }

        public class Datum
        {
            public string b64_json { get; set; }
            public string media_type { get; set; }

            public string SaveToFile(string filePath = null)
            {
                var extension = media_type == "image/jpeg" ? ".jpg" : ".png";
                if (filePath == null)
                    filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");

                var bytes = Convert.FromBase64String(b64_json);
                System.IO.File.WriteAllBytes(filePath, bytes);
                return filePath;
            }
        }

        public class ImageResponse
        {
            public int created { get; set; }
            public List<Datum> data { get; set; }
            public OpenRouterImageCreationUsage usage { get; set; }

            public string SaveToFile(string filePath = null)
            {
                if (data != null && data.Count > 0)
                    return data[0].SaveToFile(filePath);
                return null;
            }

            public static ImageResponse FromJson(string json) => JsonConvert.DeserializeObject<ImageResponse>(json, new IsoDateTimeConverter { DateTimeStyles = System.Globalization.DateTimeStyles.AssumeUniversal });
        }

        public class OpenRouterImageCreationUsage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public double cost { get; set; }
            public int total_tokens { get; set; }
        }


        public (string text, GenericAICompletions.GenericAIUsage usage) Create(
            string prompt,
            string model = "x-ai/grok-imagine-image-2.0",
            string filePath = null
            )
        {
            OpenAI.Trace(new { model, prompt}, this);

            var sw = Stopwatch.StartNew();
            var usage = new GenericAICompletions.GenericAIUsage(model, "","");
            if (base._key == null)
                base._key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
            var wc = InitWebClient();
            var response = wc.POST(__url, GetPayLoad(prompt, model));
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = ImageResponse.FromJson(response.Text);
                sw.Stop();
                usage.InputTokens = r.usage.prompt_tokens;
                usage.OutputTokens = r.usage.completion_tokens;
                usage.SetDuration(sw);

                var imageFileName = r.SaveToFile(filePath);
                OpenAI.Trace($"[IMAGE] Duration: {sw.ElapsedMilliseconds:00000} ms, Cost: {r.usage.cost:0.0000}  Model: {model}, fileName: ({imageFileName})", this);

                return (imageFileName, usage);
            }
            else throw new OpenAIAudioSpeechException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
        }

        private string GetPayLoad(string prompt, string model)
        {
            return JsonConvert.SerializeObject(new
            {
                prompt,
                model,
            });
        }
    }
}



