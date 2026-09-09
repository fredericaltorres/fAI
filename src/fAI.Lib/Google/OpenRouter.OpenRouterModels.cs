using System;
using System.Collections.Generic;

namespace fAI
{

    public partial class OpenRouter
    {
        /*
         
  curl https://openrouter.ai/api/v1/chat/completions \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $OPENROUTER_API_KEY" \
    -d '{
    "model": "deepseek/deepseek-v4-pro",
    "messages": [
      {
        "role": "user",
        "content": "what is the capital of FRANCE?"
      }
    ]
  }'
         */

        public class OpenRouterModels : HttpBase
        {
            public class AliasTarget
            {
                public string name { get; set; }
                public string slug { get; set; }
            }
            public class Architecture
            {
                public string modality { get; set; }
                public List<string> input_modalities { get; set; }
                public List<string> output_modalities { get; set; }
                public string tokenizer { get; set; }
                public object instruct_type { get; set; }
            }

            public class ArtificialAnalysis
            {
                public double? intelligence_index { get; set; }
                public double? coding_index { get; set; }
                public double? agentic_index { get; set; }
            }
            public class Benchmarks
            {
                public List<DesignArena> design_arena { get; set; }
                public ArtificialAnalysis artificial_analysis { get; set; }
            }
            public class Datum
            {
                public string id { get; set; }
                public string canonical_slug { get; set; }
                public string hugging_face_id { get; set; }
                public string name { get; set; }
                public int created { get; set; }
                public string description { get; set; }
                public int context_length { get; set; }
                public Architecture architecture { get; set; }
                public Pricing pricing { get; set; }
                public TopProvider top_provider { get; set; }
                public object per_request_limits { get; set; }
                public List<string> supported_parameters { get; set; }
                public DefaultParameters default_parameters { get; set; }
                public List<object> supported_voices { get; set; }
                public string knowledge_cutoff { get; set; }
                public object expiration_date { get; set; }
                public Links links { get; set; }
                public Reasoning reasoning { get; set; }
                public Benchmarks benchmarks { get; set; }
                public AliasTarget alias_target { get; set; }
            }
            public class DefaultParameters
            {
                public double? temperature { get; set; }
                public double? top_p { get; set; }
                public int? top_k { get; set; }
                public double? repetition_penalty { get; set; }
                public object frequency_penalty { get; set; }
                public object presence_penalty { get; set; }
            }
            public class DesignArena
            {
                public string arena { get; set; }
                public string category { get; set; }
                public int elo { get; set; }
                public double win_rate { get; set; }
                public int rank { get; set; }
            }
            public class Links
            {
                public string details { get; set; }
                public object next { get; set; }
            }
            public class Override
            {
                public int min_prompt_tokens { get; set; }
                public string prompt { get; set; }
                public string completion { get; set; }
                public string input_cache_read { get; set; }
                public string input_cache_write { get; set; }
            }
            public class Pricing
            {
                public string prompt { get; set; }
                public string completion { get; set; }
                public string image { get; set; }
                public string audio { get; set; }
                public string input_audio_cache { get; set; }
                public string web_search { get; set; }
                public string internal_reasoning { get; set; }
                public string input_cache_read { get; set; }
                public string input_cache_write { get; set; }
                public List<Override> overrides { get; set; }
                public string input_cache_write_1h { get; set; }
            }

            public class Reasoning
            {
                public bool mandatory { get; set; }
                public bool default_enabled { get; set; }
                public List<string> supported_efforts { get; set; }
                public string default_effort { get; set; }
                public bool? supports_max_tokens { get; set; }
            }
            public class TopProvider
            {
                public int? context_length { get; set; }
                public int? max_completion_tokens { get; set; }
                public bool is_moderated { get; set; }
            }

            public class GetModelsResponse
            {
                public List<Datum> data { get; set; }
                public int total_count { get; set; }
                public Links links { get; set; }
            }


            public OpenRouterModels(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
            {
            }

            public void UpdateAIModelsWithOpenRouterLatestInfo(List<AIModel> aiModels, string openRouterApiKey = null)
            {
                openRouterApiKey = openRouterApiKey ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
                var orModels = this.GetModelsDataFromOpenRouterDotCom();

                foreach (var model in aiModels)
                {
                    var orModel = orModels.data.Find(x => x.id.ToLowerInvariant() == model.Id.ToLowerInvariant());
                    if (orModel != null)
                    {
                        model.InputTokenPricePer1M = float.Parse(orModel.pricing.prompt) * 1000000f;
                        model.OutputTokenPricePer1M = float.Parse(orModel.pricing.completion) * 1000000f;
                        model.ContextLength = orModel.context_length;
                        model.ReleaseDate = DateTimeOffset.FromUnixTimeSeconds(orModel.created).DateTime;
                        if (DateTime.TryParse(orModel.knowledge_cutoff, out DateTime knowledgeCutoff))
                            model.KnowledgeCutoff = knowledgeCutoff;
                    }
                }
            }

            public GetModelsResponse GetModelsDataFromOpenRouterDotCom()
            {
                try { 
                    var url = "https://openrouter.ai/api/v1/models/user";
                    var response = InitWebClient().GET(url);
                    var modelsResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<GetModelsResponse>(response.Text);
                    return modelsResponse;
                }
                catch (Exception ex)
                {
                    HttpBase.Trace(new { ex }, this);
                    return new GetModelsResponse { data = new List<Datum>(), total_count = 0, links = new Links() };    
                }
            }
        }
    }
}
