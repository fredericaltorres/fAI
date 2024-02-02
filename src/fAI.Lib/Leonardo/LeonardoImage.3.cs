using NAudio.SoundFont;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using static fAI.OpenAIImage;

namespace fAI
{
    public partial class LeonardoImage : HttpBase
    {
        public class GeneratedImage
        {
            public string url { get; set; }
            public bool nsfw { get; set; }
            public string id { get; set; }
            public int likeCount { get; set; }
            public object motionMP4URL { get; set; }
            public List<object> generated_image_variation_generics { get; set; }

            public Stopwatch Stopwatch { get; set; }

            public static GeneratedImage FromJson(string text)
            {
                return JsonUtils.FromJSON<GeneratedImage>(text);
            }
        }

        public class GenerationsByPk
        {
            public List<GeneratedImage> generated_images { get; set; }
            public string modelId { get; set; }
            public object motion { get; set; }
            public object motionModel { get; set; }
            public object motionStrength { get; set; }
            public string prompt { get; set; }
            public string negativePrompt { get; set; }
            public int imageHeight { get; set; }
            public object imageToVideo { get; set; }
            public int imageWidth { get; set; }
            public int inferenceSteps { get; set; }
            public int seed { get; set; }
            public bool @public { get; set; }
            public string scheduler { get; set; }
            public string sdVersion { get; set; }
            public string status { get; set; }
            public object presetStyle { get; set; }
            public object initStrength { get; set; }
            public int guidanceScale { get; set; }
            public string id { get; set; }
            public DateTime createdAt { get; set; }
            public bool promptMagic { get; set; }
            public object promptMagicVersion { get; set; }
            public object promptMagicStrength { get; set; }
            public bool photoReal { get; set; }
            public object photoRealStrength { get; set; }
            public object fantasyAvatar { get; set; }
            public List<object> generation_elements { get; set; }
        }

        public class GenerationResultResponse : BaseHttpResponse
        {
            public GenerationsByPk generations_by_pk { get; set; }

            public bool Completed => this.generations_by_pk.status == "COMPLETE";

            public static GenerationResultResponse FromJson(string text)
            {
                return JsonUtils.FromJSON<GenerationResultResponse>(text);
            }

            public List<string> DownloadImages()
            {
                var r = new List<string>();

                foreach (var d in this.generations_by_pk.generated_images)
                    r.Add(base.DownloadImage(new Uri(d.url)));

                return r;
            }
        }
    }
}

