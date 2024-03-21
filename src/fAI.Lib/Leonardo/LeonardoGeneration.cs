using NAudio.SoundFont;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using static fAI.OpenAIImage;
using static fAI.LeonardoImage;
using DynamicSugar;

namespace fAI
{
    public partial class LeonardoGeneration  
    {
        public List<GenerationSub> generations { get; set; }

        public string Json { get; set; }

        public static LeonardoGeneration FromJson(string text)
        {
            var o = JsonUtils.FromJSON<LeonardoGeneration>(text);
            o.Json = text;
            return o;
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class GeneratedImage
        {
            public string url { get; set; }
            public bool nsfw { get; set; }
            public string id { get; set; }
            public int likeCount { get; set; }
            public object motionMP4URL { get; set; }
            public List<GeneratedImageVariationGeneric> generated_image_variation_generics { get; set; }
        }

        public class GeneratedImageVariationGeneric
        {
            public string url { get; set; }
            public string id { get; set; }
            public string status { get; set; }
            public string transformType { get; set; }
        }

        public class GenerationsByPk2
        {
            public string Json { get; set; }

            public GenerationSub generations_by_pk { get; set; }

            public static GenerationSub FromJson(string text)
            {
                var o = JsonUtils.FromJSON<GenerationsByPk2>(text);
                o.Json = text;
                return o.generations_by_pk;
            }
        }

        public class GenerationSub
        {
            public string Json { get; set; }

            public static GenerationSub FromJson(string text)
            {
                var o = JsonUtils.FromJSON<GenerationSub>(text);
                o.Json = text;
                return o;
            }

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
            public int? inferenceSteps { get; set; }
            public int seed { get; set; }
            public bool @public { get; set; }
            public string scheduler { get; set; }
            public string sdVersion { get; set; }
            public string status { get; set; }
            public string presetStyle { get; set; }
            public object initStrength { get; set; }
            public int? guidanceScale { get; set; }
            public string id { get; set; }
            public DateTime createdAt { get; set; }
            public bool promptMagic { get; set; }
            public object promptMagicVersion { get; set; }
            public object promptMagicStrength { get; set; }
            public bool photoReal { get; set; }
            public object photoRealStrength { get; set; }
            public object fantasyAvatar { get; set; }
            public List<GenerationElement> generation_elements { get; set; }


            private static string ProcessJsonString(object o)
            {
                var s = o as string;
                if (s == null)
                    return null;
                return s.Replace("\"", "\\\"");
            }
            public string GetPromptParametersInPocoFormat() 
            {
                var ignoreProperties = DS.List("id", "createdAt", "generated_images", "generation_elements");
                var sb = new System.Text.StringBuilder();
                var d = DS.Dictionary(this);
                sb.AppendLine("new {");
                foreach(var k in d.Keys)
                {
                    if (ignoreProperties.Contains(k))
                        continue;

                    var val = "";
                    if (d[k] == null)
                    {
                        val = "null";
                    }
                    else if (d[k].GetType().Name == "String")
                    {
                        val = $@"""{ProcessJsonString(d[k])}""";
                    }
                    else if (d[k].GetType().Name == "Boolean")
                    {
                        val = d[k].ToString().ToLowerInvariant();
                    }
                    else
                    {
                        val = d[k].ToString();
                    }
                    sb.Append($"{k} = {val}, ");
                }
                sb.Remove(sb.Length - 2, 2);
                sb.AppendLine("new }");
                return sb.ToString();
            }
        }

        public class GenerationElement
        {
            public int id { get; set; }
            public double weightApplied { get; set; }
            public Lora lora { get; set; }
        }

        public class Lora
        {
            public string akUUID { get; set; }
            public string baseModel { get; set; }
            public string description { get; set; }
            public string name { get; set; }
            public string urlImage { get; set; }
            public double weightDefault { get; set; }
            public double weightMax { get; set; }
            public double weightMin { get; set; }
            public double weight { get; set; } = 0.6;
        }
    }
}
