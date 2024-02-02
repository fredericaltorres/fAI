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

