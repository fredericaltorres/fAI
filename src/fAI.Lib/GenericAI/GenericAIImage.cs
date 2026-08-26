using DynamicSugar;
using fAI.Util.Strings;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public List<string> GetModels()
        {
            return DS.List(
               "x-ai/grok-imagine-image-2.0",
               "qwen/qwen-image-3-pro"
                );
        }

        public class Datum
        {
            public string b64_json { get; set; }

            public string SaveToFile()
            {
                var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
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

            public string SaveToFile()
            {
                if (data != null && data.Count > 0)
                    return data[0].SaveToFile();
                return null;
            }

            public static ImageResponse FromJson(string json) => JsonConvert.DeserializeObject<ImageResponse>(json, new IsoDateTimeConverter { DateTimeStyles = System.Globalization.DateTimeStyles.AssumeUniversal });
        }

        public class OpenRouterImageCreationUsage
        {
            public int completion_tokens { get; set; }
            public double cost { get; set; }
            public int prompt_tokens { get; set; }
            public int total_tokens { get; set; }
        }


        public (string text, GenericAICompletions.GenericAIUsage usage) Create(
            string prompt,
            string model = "x-ai/grok-imagine-image-2.0"
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
                OpenAI.Trace($"[IMAGE] Duration: {sw.ElapsedMilliseconds:00000} ms, Cost: {r.usage.cost:0.0000}  Model: {model}", this);

                return ("", usage);
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



