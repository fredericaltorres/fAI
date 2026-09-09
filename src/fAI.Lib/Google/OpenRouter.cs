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

    public partial class OpenRouter : HttpBase
    {
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

                new AIModel { Id = "google/gemini-3.8-flash",           InputTokenPricePer1M = 0.75f,  OutputTokenPricePer1M = 3.75f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 9, 2),   KnowledgeCutoff = new DateTime(2026, 9, 2) },

                new AIModel { Id = "meta/muse-spark-1.2",               InputTokenPricePer1M = 1.25f,  OutputTokenPricePer1M = 4.25f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 8, 5),   KnowledgeCutoff = new DateTime(2026, 8, 5) },

                new AIModel { Id = "openai/gpt-6-astra",                InputTokenPricePer1M = 10.00f, OutputTokenPricePer1M = 50.00f,  ContextLength = 1_100_000, ReleaseDate = new DateTime(2026, 9, 4),  KnowledgeCutoff = new DateTime(2026, 9, 4) },
                new AIModel { Id = "openai/gpt-5.5",                    InputTokenPricePer1M = 5.00f,  OutputTokenPricePer1M = 30.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 4, 24),  KnowledgeCutoff = new DateTime(2025, 12, 1) },
                new AIModel { Id = "openai/gpt-5-mini",                 InputTokenPricePer1M = 0.25f,  OutputTokenPricePer1M = 2.00f,   ContextLength = 400_000,   ReleaseDate = new DateTime(2025, 8, 7),   KnowledgeCutoff = new DateTime(2024, 5, 1) },
                new AIModel { Id = "openai/gpt-5.6-luna",               InputTokenPricePer1M = 0.10f,  OutputTokenPricePer1M = 0.60f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 7, 9),   KnowledgeCutoff = new DateTime(2026, 2, 1) },
                new AIModel { Id = "openai/gpt-5.6-terra",              InputTokenPricePer1M = 1.00f,  OutputTokenPricePer1M = 6.00f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 7, 9),   KnowledgeCutoff = new DateTime(2026, 2, 1) },
                new AIModel { Id = "openai/gpt-5.6-sol",                InputTokenPricePer1M = 5.00f,  OutputTokenPricePer1M = 30.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 7, 9),   KnowledgeCutoff = new DateTime(2026, 2, 1) },
                new AIModel { Id = "anthropic/claude-fable-5",          InputTokenPricePer1M = 10.00f, OutputTokenPricePer1M = 50.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 6, 9)  },
                new AIModel { Id = "anthropic/claude-opus-5",           InputTokenPricePer1M = 5.00f,  OutputTokenPricePer1M = 25.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 7, 24) },
                new AIModel { Id = "anthropic/claude-opus-4.7",         InputTokenPricePer1M = 5.00f,  OutputTokenPricePer1M = 25.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 4, 16) },
                //new AIModel { Id = "anthropic/claude-opus-4.7-fast",    InputTokenPricePer1M = 30.00f, OutputTokenPricePer1M = 150.00f, ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 5, 12) },
                new AIModel { Id = "anthropic/claude-opus-4.6",         InputTokenPricePer1M = 5.00f,  OutputTokenPricePer1M = 25.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 2, 4)  },
                new AIModel { Id = "anthropic/claude-sonnet-4.5",       InputTokenPricePer1M = 3.00f,  OutputTokenPricePer1M = 15.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2025, 9, 29),  KnowledgeCutoff = new DateTime(2025, 1, 1) },
                new AIModel { Id = "anthropic/claude-sonnet-4.6",       InputTokenPricePer1M = 3.00f,  OutputTokenPricePer1M = 15.00f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 2, 17) },
                new AIModel { Id = "anthropic/claude-haiku-4.5",        InputTokenPricePer1M = 1.00f,  OutputTokenPricePer1M = 5.00f,   ContextLength = 200_000,   ReleaseDate = new DateTime(2025, 10, 15) },
                new AIModel { Id = "mistralai/mistral-small-2603",      InputTokenPricePer1M = 0.15f,  OutputTokenPricePer1M = 0.60f,   ContextLength = 262_000,   ReleaseDate = new DateTime(2026, 3, 16) },
                new AIModel { Id = "mistralai/mistral-medium-3.1",      InputTokenPricePer1M = 0.40f,  OutputTokenPricePer1M = 2.00f,   ContextLength = 131_000,   ReleaseDate = new DateTime(2025, 8, 13),  KnowledgeCutoff = new DateTime(2025, 6, 1) },
                new AIModel { Id = "mistralai/mistral-medium-3-5",      InputTokenPricePer1M = 1.50f,  OutputTokenPricePer1M = 7.50f,   ContextLength = 262_000,   ReleaseDate = new DateTime(2026, 4, 30) },
                new AIModel { Id = "mistralai/mistral-medium-3",        InputTokenPricePer1M = 0.40f,  OutputTokenPricePer1M = 2.00f,   ContextLength = 131_000 },
                new AIModel { Id = "mistralai/mistral-large-2512",      InputTokenPricePer1M = 0.50f,  OutputTokenPricePer1M = 1.50f,   ContextLength = 262_000,   ReleaseDate = new DateTime(2025, 12, 1) },
                new AIModel { Id = "x-ai/grok-4.6",                     InputTokenPricePer1M = 2.00f,  OutputTokenPricePer1M = 6.00f,   ContextLength = 500_000,   ReleaseDate = new DateTime(2026, 7, 8)  },
                new AIModel { Id = "x-ai/grok-4.5",                     InputTokenPricePer1M = 2.00f,  OutputTokenPricePer1M = 6.00f,   ContextLength = 500_000,   ReleaseDate = new DateTime(2026, 7, 8)  },
                new AIModel { Id = "x-ai/grok-4.20",                    InputTokenPricePer1M = 1.25f,  OutputTokenPricePer1M = 2.50f,   ContextLength = 2_000_000 },
                new AIModel { Id = "x-ai/grok-4.3",                     InputTokenPricePer1M = 1.25f,  OutputTokenPricePer1M = 2.50f,   ContextLength = 1_000_000 },
                new AIModel { Id = "deepseek/deepseek-v4-flash",        InputTokenPricePer1M = 0.084f, OutputTokenPricePer1M = 0.168f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 4, 23) },
                new AIModel { Id = "deepseek/deepseek-v4-pro",          InputTokenPricePer1M = 0.435f, OutputTokenPricePer1M = 0.87f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 4, 23) },
                new AIModel { Id = "thinkingmachines/inkling",          InputTokenPricePer1M = 1.00f,  OutputTokenPricePer1M = 4.05f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 7, 17) },
                new AIModel { Id = "moonshotai/kimi-k2.6",              InputTokenPricePer1M = 0.55f,  OutputTokenPricePer1M = 3.20f,   ContextLength = 262_000 },
                new AIModel { Id = "moonshotai/kimi-k3",                InputTokenPricePer1M = 2.81f,  OutputTokenPricePer1M = 14.01f,  ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 7, 16) },
                new AIModel { Id = "qwen/qwen3.8-max",                  InputTokenPricePer1M = 2.00f,  OutputTokenPricePer1M = 6.00f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 8, 3)  },

                new AIModel { Id = "poolside/laguna-s-2.1",             InputTokenPricePer1M = 0.09f,  OutputTokenPricePer1M = 0.18f,   ContextLength = 1_000_000, ReleaseDate = new DateTime(2026, 7, 21)  },
                new AIModel { Id = "poolside/laguna-xs-2.1",            InputTokenPricePer1M = 0.06f,  OutputTokenPricePer1M = 0.12f,   ContextLength = 262*1024,  ReleaseDate = new DateTime(2026, 7, 2)  },
                // https://poolside.ai/models
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
