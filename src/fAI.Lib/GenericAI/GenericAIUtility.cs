using Newtonsoft.Json;
using System;

namespace fAI
{
    public class GenericAIUtility: HttpBase
    {
        //https://openrouter.ai/docs/api/api-reference/stt/create-transcription
        public const string __url = "https://openrouter.ai/api/v1/audio/transcriptions";

        public GenericAIUtility(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }

        public class CreditData
        {
            [JsonProperty("total_credits")]
            public float TotalCredits { get; set; }

            [JsonProperty("total_usage")]
            public float TotalUsage { get; set; }

            public float CreditsRemaining => TotalCredits - TotalUsage;
        }

        public class CreditResponse
        {
            public CreditData data { get; set; }

            public static CreditResponse FromJson(string json) => JsonConvert.DeserializeObject<CreditResponse>(json);
        }


        public CreditData GetCredits()
        {
            OpenAI.Trace(new { }, this);

            if (base._key == null)
                base._key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

            var wc = InitWebClient();
            var response = wc.GET("https://openrouter.ai/api/v1/credits");
            if (response.Success)
            {
                var r = CreditResponse.FromJson(response.Text);
                Logger.Trace(response.Text, this);
                return r.data;
            }
            else throw new OpenAIAudioSpeechException($"{nameof(GetCredits)}() failed - {response.Exception.Message}", response.Exception);
        }
    }
}

