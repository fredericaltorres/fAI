using DynamicSugar;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;

namespace fAI
{
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
        public static List<string> GetModels()
        {
            return DS.List(

                "google/gemini-3.1-flash-lite",     // $0.25 / $1.50 per 1M, Context 1M, Released May 7, 2026
                "google/gemini-3.5-flash",          // $1.50 / $9per 1M, Context 1M Released May 19, 2026 Knowledge Cutoff Jan 2025

                "openai/gpt-5.5",                   // $5 / $30per 1M, Context 1M, Released Apr 24, 2026, Knowledge Cutoff Dec 2025
                "openai/gpt-5-mini",                // $0.25 / $2per 1M, Context, 400K, Released, Aug 7, 2025, Knowledge Cutoff May 2024
                "openai/gpt-5.6-luna",              // $1 / $6per 1M, Context, 1M, Released, Jul 9, 2026, Knowledge Cutoff Feb 2026
                "openai/gpt-5.6-terra",             // $2.50 / $15per 1M, Context, 1M, Released, Jul 9, 2026, Knowledge Cutoff Feb 2026
                "openai/gpt-5.6-sol",               // $5 / $30 per 1M, Context, 1M, Released, Jul 9, 2026, Knowledge Cutoff Feb 2026

                "anthropic/claude-opus-4.7",        // $5 / $25 per 1M, Context 1M, Released Apr 16, 2026
                "anthropic/claude-opus-4.7-fast",   // $30 / $150per 1M, Context 1M, Released May 12, 2026
                "anthropic/claude-opus-4.6",        // $5 / $25 per 1M, Context 1M, Released Feb 4, 2026

                "anthropic/claude-sonnet-4.5",      // $3 / $15per 1M, Context 1M, Released Sep 29, 2025, Knowledge Cutoff Jan 2025
                "anthropic/claude-sonnet-4.6",      // $3 / $15per 1M, Context 1M, Released Feb 17, 2026
                "anthropic/claude-haiku-4.5",       // $1 / $5per 1M, Context 200K, Released Oct 15, 2025

                "mistralai/mistral-small-2603",     // Mistral: Mistral Small 4, $0.15 / $0.60 per 1M, Context , 262K Released Mar 16, 2026
                "mistralai/mistral-medium-3.1",     // $0.40 / $2 per 1M, Context 131K, Released Aug 13, 2025, Knowledge Cutoff Jun 2025
                "mistralai/mistral-medium-3-5",     // $1.50 / $7.50per 1M, Context 262K, Released Apr 30, 2026
                "mistralai/mistral-medium-3",       // $0.40 / $ 2per 1M. Context 131K
                "mistralai/mistral-large-2512",     // Mistral: Mistral Large 3 2512

                "x-ai/grok-4.5",                    // $2 / $6per 1M, Context 500K, Released Jul 8, 2026
                "x-ai/grok-4.20",                   // $1.25 / $2.50per 1M, Context 2M
                "x-ai/grok-4.3",                    // $1.25 / $2.50per 1M, Context 1M

                "deepseek/deepseek-v4-flash",       // $0.084 / $0.168 per 1M, Context, 1M Released Apr 23, 2026
                "deepseek/deepseek-v4-pro",         // $0.435 / $0.87per 1M, Context 1M, Released Apr 23, 2026

                //"mistralai/mistral-large-2512",
                //"mistralai/mistral-medium-3.1",

                //"minimax/minimax-m3",  TOO SLOW
                //"minimax/minimax-m2.5",
                //"minimax/minimax-m2.1",
                //"minimax/minimax-m2",

                //"nvidia/nemotron-3-super-120b-a12b:free",
                //"nvidia/nemotron-3-ultra-550b-a55b:free",
                //"nvidia/nemotron-3.5-content-safety:free", // LIMITED No bullet point or Translate
                //"nvidia/nemotron-nano-9b-v2:free",

                "moonshotai/kimi-k2.6",//    $0.55 / $3.20per 1M, Context 262K
                "moonshotai/kimi-k3",  // $3 / $15per 1M, Context 1M, Released Jul 16, 2026

                //"moonshotai/kimi-k2.7-code", TOO SLOW  
                //"moonshotai/kimi-k2.5",
                //"moonshotai/kimi-k2-thinking",

                //"qwen/qwen3.7-plus", TOO SLOW
                //"qwen/qwen3.6-35b-a3b",
                //"qwen/qwen3.6-flash",
                //"qwen/qwen3.6-plus",
                //"qwen/qwen3-next-80b-a3b-thinking",
                // "qwen/qwen3-next-80b-a3b-instruct:free", LIMITED

                //"deepseek/deepseek-v3.2",
                "deepseek/deepseek-v3.1-terminus",
                //"deepseek/deepseek-chat-v3.1",

                //"google/gemma-4-26b-a4b-it:free", 
                //"google/gemma-4-31b-it:free",
                "amazon/nova-2-lite-v1"
                );
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
