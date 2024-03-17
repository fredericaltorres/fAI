using NAudio.SoundFont;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using static fAI.OpenAIImage;
using DynamicSugar;

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


        public class LeonardoJsonError
        {
            public string error { get; set; }
            public string path { get; set; }
            public string code { get; set; }

            public static LeonardoJsonError FromJson(string text)
            {
                return JsonUtils.FromJSON<LeonardoJsonError>(text);
            }

            public LeonardoException GetLeonardoException()
            {
                return new LeonardoException($"{error}, path={path}, code={code}");
            }

            public override string ToString()
            {
                return DS.Dictionary(this).Format();
            }
        }

    }
}

