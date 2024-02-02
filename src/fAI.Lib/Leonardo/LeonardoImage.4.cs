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
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class DeleteGenerationsByPk
        {
            public string id { get; set; }
        }

        public class DeleteGenerationsResponse : BaseHttpResponse
        {
            public DeleteGenerationsByPk delete_generations_by_pk { get; set; }

            public string GenerationId => delete_generations_by_pk.id;  

            public static DeleteGenerationsResponse FromJson(string text)
            {
                return JsonUtils.FromJSON<DeleteGenerationsResponse>(text);
            }
        }


    }
}

