using DynamicSugar;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;

namespace fAI
{
    public class AIModel 
    {
        public string Id { get; set; }
        public float InputTokenPricePer1M { get; set; }
        public float OutputTokenPricePer1M { get; set; }
        public int ContextLength { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime KnowledgeCutoff { get; set; }

        public override string ToString()
        {
            //return $"Model: {Name}, Input: ${InputTokenPricePer1M}/1M, Output: ${OutputTokenPricePer1M}/1M, Context Length: {ContextLength}, Release Date: {ReleaseDate.ToShortDateString()}, Knowledge Cutoff: {KnowledgeCutoff.ToShortDateString()}";
            return this.Id;
        }

        public override bool Equals(object obj)
        {
            var z = obj as AIModel;
            if (z == null) return false;
            return this.Id.ToLowerInvariant() == z.Id.ToLowerInvariant();
        }

        public float ComputeCost(int inputTokens, int outputTokens)
        {
            float inputCost = (inputTokens / 1_000_000f) * InputTokenPricePer1M;
            float outputCost = (outputTokens / 1_000_000f) * OutputTokenPricePer1M;
            return inputCost + outputCost;
        }
    }

    public class OpenRouter : HttpBase
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

        public static List<AIModel> __modelsLoadedAndUpdated = null;

        public static void ClearOpenRouterModelsCache()
        {
            __modelsLoadedAndUpdated = null;
        }

        // openrouter api
        // https://openrouter.ai/docs/api_reference/overview
        public static List<AIModel> GetModels(string openRouterApiKey = null)
        {
            if(__modelsLoadedAndUpdated != null)
                return __modelsLoadedAndUpdated;

            var aiModels = new List<AIModel>
            {
                new AIModel { Id = "google/gemini-3.1-flash-lite",      InputTokenPricePer1M = 0.25f,  OutputTokenPricePer1M = 1.50f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 5, 7)  },
                new AIModel { Id = "google/gemini-3.5-flash",           InputTokenPricePer1M = 1.50f,  OutputTokenPricePer1M = 9.00f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 5, 19),  KnowledgeCutoff = new DateTime(2025, 1, 1) },
                new AIModel { Id = "openai/gpt-5.5",                    InputTokenPricePer1M = 5.00f,  OutputTokenPricePer1M = 30.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 4, 24),  KnowledgeCutoff = new DateTime(2025, 12, 1) },
                new AIModel { Id = "openai/gpt-5-mini",                 InputTokenPricePer1M = 0.25f,  OutputTokenPricePer1M = 2.00f,   ContextLength = 400_000,   ReleaseDate = new DateTime(2025, 8, 7),   KnowledgeCutoff = new DateTime(2024, 5, 1) },
                new AIModel { Id = "openai/gpt-5.6-luna",               InputTokenPricePer1M = 0.10f,  OutputTokenPricePer1M = 0.60f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 7, 9),   KnowledgeCutoff = new DateTime(2026, 2, 1) },
                new AIModel { Id = "openai/gpt-5.6-terra",              InputTokenPricePer1M = 1.00f,  OutputTokenPricePer1M = 6.00f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 7, 9),   KnowledgeCutoff = new DateTime(2026, 2, 1) },
                new AIModel { Id = "openai/gpt-5.6-sol",                InputTokenPricePer1M = 5.00f,  OutputTokenPricePer1M = 30.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 7, 9),   KnowledgeCutoff = new DateTime(2026, 2, 1) },
                new AIModel { Id = "anthropic/claude-fable-5",          InputTokenPricePer1M = 10.00f, OutputTokenPricePer1M = 50.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 6, 9)  },
                new AIModel { Id = "anthropic/claude-opus-5",           InputTokenPricePer1M = 5.00f,  OutputTokenPricePer1M = 25.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 7, 24) },
                new AIModel { Id = "anthropic/claude-opus-4.7",         InputTokenPricePer1M = 5.00f,  OutputTokenPricePer1M = 25.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 4, 16) },
                new AIModel { Id = "anthropic/claude-opus-4.7-fast",    InputTokenPricePer1M = 30.00f, OutputTokenPricePer1M = 150.00f, ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 5, 12) },
                new AIModel { Id = "anthropic/claude-opus-4.6",         InputTokenPricePer1M = 5.00f,  OutputTokenPricePer1M = 25.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 2, 4)  },
                new AIModel { Id = "anthropic/claude-sonnet-4.5",       InputTokenPricePer1M = 3.00f,  OutputTokenPricePer1M = 15.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2025, 9, 29),  KnowledgeCutoff = new DateTime(2025, 1, 1) },
                new AIModel { Id = "anthropic/claude-sonnet-4.6",       InputTokenPricePer1M = 3.00f,  OutputTokenPricePer1M = 15.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 2, 17) },
                new AIModel { Id = "anthropic/claude-haiku-4.5",        InputTokenPricePer1M = 1.00f,  OutputTokenPricePer1M = 5.00f,   ContextLength = 200_000,   ReleaseDate = new DateTime(2025, 10, 15) },
                new AIModel { Id = "mistralai/mistral-small-2603",      InputTokenPricePer1M = 0.15f,  OutputTokenPricePer1M = 0.60f,   ContextLength = 262_000,   ReleaseDate = new DateTime(2026, 3, 16) },
                new AIModel { Id = "mistralai/mistral-medium-3.1",      InputTokenPricePer1M = 0.40f,  OutputTokenPricePer1M = 2.00f,   ContextLength = 131_000,   ReleaseDate = new DateTime(2025, 8, 13),  KnowledgeCutoff = new DateTime(2025, 6, 1) },
                new AIModel { Id = "mistralai/mistral-medium-3-5",      InputTokenPricePer1M = 1.50f,  OutputTokenPricePer1M = 7.50f,   ContextLength = 262_000,   ReleaseDate = new DateTime(2026, 4, 30) },
                new AIModel { Id = "mistralai/mistral-medium-3",        InputTokenPricePer1M = 0.40f,  OutputTokenPricePer1M = 2.00f,   ContextLength = 131_000 },
                new AIModel { Id = "mistralai/mistral-large-2512",      InputTokenPricePer1M = 0.50f,  OutputTokenPricePer1M = 1.50f,   ContextLength = 262_000,   ReleaseDate = new DateTime(2025, 12, 1) },
                new AIModel { Id = "x-ai/grok-4.5",                     InputTokenPricePer1M = 2.00f,  OutputTokenPricePer1M = 6.00f,   ContextLength = 500_000,   ReleaseDate = new DateTime(2026, 7, 8)  },
                new AIModel { Id = "x-ai/grok-4.20",                    InputTokenPricePer1M = 1.25f,  OutputTokenPricePer1M = 2.50f,   ContextLength = 2_000_000 },
                new AIModel { Id = "x-ai/grok-4.3",                     InputTokenPricePer1M = 1.25f,  OutputTokenPricePer1M = 2.50f,   ContextLength = 1_000_000 },
                new AIModel { Id = "deepseek/deepseek-v4-flash",        InputTokenPricePer1M = 0.084f, OutputTokenPricePer1M = 0.168f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 4, 23) },
                new AIModel { Id = "deepseek/deepseek-v4-pro",          InputTokenPricePer1M = 0.435f, OutputTokenPricePer1M = 0.87f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 4, 23) },
                new AIModel { Id = "thinkingmachines/inkling",          InputTokenPricePer1M = 1.00f,  OutputTokenPricePer1M = 4.05f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 7, 17) },
                new AIModel { Id = "moonshotai/kimi-k2.6",              InputTokenPricePer1M = 0.55f,  OutputTokenPricePer1M = 3.20f,   ContextLength = 262_000 },
                new AIModel { Id = "moonshotai/kimi-k3",                InputTokenPricePer1M = 2.81f,  OutputTokenPricePer1M = 14.01f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 7, 16) },
                new AIModel { Id = "qwen/qwen3.8-max",                  InputTokenPricePer1M = 2.00f,  OutputTokenPricePer1M = 6.00f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 8, 3)  },
            };

            openRouterApiKey = openRouterApiKey ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
            var orm = new OpenRouterModels(apiKey: openRouterApiKey);
            orm.UpdateAIModelsWithOpenRouterLatestInfo(aiModels, openRouterApiKey);
            __modelsLoadedAndUpdated = aiModels;
            return aiModels;

            //    /*
            //        provider: {
            //            order: ["moonshotai/mxfp4",  "baseten/fp8"],
            //            allow_fallbacks: false
            //        }
            //     */
            //    );
        }

        public OpenRouter(int timeOut = -1, string apiKey = null)
        {
            base._key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;

            if (apiKey != null)
                base._key = apiKey;
        }

        public OpenRouterCompletions _completions = null;
        public OpenRouterCompletions Completions => _completions ?? (_completions = new OpenRouterCompletions(apiKey: base._key));
    }
}
