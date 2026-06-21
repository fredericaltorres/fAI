using DynamicSugar;
using System;
using System.Collections.Generic;

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
                //"mistralai/mistral-medium-3-5",
                "mistralai/mistral-small-2603",
                "mistralai/ministral-14b-2512",
                "mistralai/ministral-8b-2512",
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

                //"moonshotai/kimi-k2.7-code", TOO SLOW  
                //"moonshotai/kimi-k2.5",
                //"moonshotai/kimi-k2-thinking",

                //"qwen/qwen3.7-plus", TOO SLOW
                //"qwen/qwen3.6-35b-a3b",
                //"qwen/qwen3.6-flash",
                //"qwen/qwen3.6-plus",
                //"qwen/qwen3-next-80b-a3b-thinking",
                // "qwen/qwen3-next-80b-a3b-instruct:free", LIMITED

                "deepseek/deepseek-v4-flash", 
                "deepseek/deepseek-v4-pro",
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
