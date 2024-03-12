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
            public double weightApplied { get { return -0.6; } }
        }

        public class GenerationElement
        {
            public int id { get; set; }
            public double weightApplied { get; set; }
            public Lora lora { get; set; }

            public static GenerationElement FromJson(string text)
            {
                return JsonUtils.FromJSON<GenerationElement>(text);
            }

            public static GenerationElement GetGlasscore()
            {
                return FromJson(@" 
                {
                ""id"": 18278835,
                ""weightApplied"": -0.6,
                ""lora"": {
                    ""akUUID"": ""a699f5da-f7f5-4afe-8473-c426b245c145"",
                    ""baseModel"": ""SDXL_1_0"",
                    ""description"": ""Craft realistic glass objects. Use \""glass\"" in the prompt with Leonardo Vision XL for best results."",
                    ""name"": ""Glasscore"",
                    ""urlImage"": ""https://cdn.leonardo.ai/element_thumbnails/a699f5da-f7f5-4afe-8473-c426b245c145/thumbnail.png"",
                    ""weightDefault"": 1.0,
                    ""weightMax"": 2.0,
                    ""weightMin"": -2.0
                    }
                }
                ");
            }
        }


        public class GenerationResponse : BaseHttpResponse
        {
            public string GenerationId => sdGenerationJob.generationId;

            public SdGenerationJob sdGenerationJob { get; set; }


            public static GenerationResponse FromJson(string text)
            {
                return JsonUtils.FromJSON<GenerationResponse>(text);
            }
        }

        public class SdGenerationJob
        {
            public string generationId { get; set; }
            public int apiCreditCost { get; set; }
        }

        public class UserInformation : BaseHttpResponse
        {
            public List<UserDetail> user_details { get; set; }

            public static UserInformation FromJson(string text)
            {
                return JsonUtils.FromJSON<UserInformation>(text);
            }
        }

        public class User
        {
            public string id { get; set; }
            public string username { get; set; }
        }

        public class UserDetail
        {
            public User user { get; set; }
            public DateTime tokenRenewalDate { get; set; }
            public int subscriptionTokens { get; set; }
            public int subscriptionGptTokens { get; set; }
            public int subscriptionModelTokens { get; set; }
            public int apiConcurrencySlots { get; set; }
        }

       
    }
}

