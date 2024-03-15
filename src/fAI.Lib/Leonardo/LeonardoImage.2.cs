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
        //public class Lora
        //{
        //    public string akUUID { get; set; }
        //    public string baseModel { get; set; }
        //    public string description { get; set; }
        //    public string name { get; set; }
        //    public string urlImage { get; set; }
        //    public double weightDefault { get; set; }
        //    public double weightMax { get; set; }
        //    public double weightMin { get; set; }
        //    public double weightApplied { get; set; } = -0.6;
        //    public double weight { get; set; } = -0.6;
        //}

        public class GenerationElementRoot
        {
            public List<GenerationElement> loras { get; set; }

            public static GenerationElementRoot FromJson(string text)
            {
                return JsonUtils.FromJSON<GenerationElementRoot>(text);
            }
        }

        public class GenerationElement
        {
            public string akUUID { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string urlImage { get; set; }
            public string baseModel { get; set; }
            public double weightDefault { get; set; }
            
            public double weightMin { get; set; }
            public double weightMax { get; set; }
            public string creatorName { get; set; }

            public static List<GenerationElement> FromJson(string text)
            {
                return JsonUtils.FromJSON<List<GenerationElement>>(text);
            }
        }

        public class GenerationElementForBody
        {
            public string akUUID { get; set; }
            public double weight { get; set; }
            //public string name { get; set; }
            //public string description { get; set; }
            //public string urlImage { get; set; }
            //public string baseModel { get; set; }
            //public double weightDefault { get; set; }

            //public double weightMin { get; set; }
            //public double weightMax { get; set; }
            //public string creatorName { get; set; }

            public static GenerationElementForBody FromJson(GenerationElement e)
            {
                return new GenerationElementForBody { 
                    akUUID = e.akUUID, 
                    weight = e.weightDefault
                    //  name = e.name, 
                    //description = e.description, 
                    //urlImage = e.urlImage,
                    //baseModel = e.baseModel, 
                    //weightDefault = e.weightDefault, 
                    //weightMin = e.weightMin, 
                    //weightMax = e.weightMax, 
                    // creatorName = e.creatorName 
                };     
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





        public class CustomModel
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public bool nsfw { get; set; }
            public bool featured { get; set; }
            public ModelGeneratedImage generated_image { get; set; }

            public override string ToString()
            {
                return $"name:{name}, description:{description}";
            }
        }

        public class ModelGeneratedImage
        {
            public string id { get; set; }
            public string url { get; set; }
        }

        public class Models
        {
            public List<CustomModel> custom_models { get; set; }

            public static Models FromJson(string text)
            {
                return JsonUtils.FromJSON<Models>(text);
            }
        }
    }
}

