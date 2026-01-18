using DynamicSugar;
using System;
using System.Collections.Generic;

namespace fAI
{
    public class Anthropic : HttpBase
    {
        public static string AnthropicApiVersion = "2023-06-01" ;

        public static List<string> GetModels()
        {
            return DS.List(
                "claude-sonnet-4-5", 
                "claude-haiku-4-5",
                "claude-opus-4-1",
                "claude-opus-4-0",
                "claude-3-7-sonnet-latest",
                "claude-3-5-haiku-latest"
            );
        }

        public Anthropic(int timeOut = -1, string key = null)
        {
            base._key = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            if (key != null)
                base._key = key;

            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;
        }
        
        public AnthropicCompletions _completions = null;
        public AnthropicCompletions Completions => _completions ?? (_completions = new AnthropicCompletions( openAiKey: base._key));
    }

    public class Mistral : HttpBase
    {

        public Mistral(int timeOut = -1)
        {
            base._key = Environment.GetEnvironmentVariable("MISTRAL_API_KEY");
            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;
        }

        public MistralCompletions _completions = null;
        public MistralCompletions Completions => _completions ?? (_completions = new MistralCompletions());

        
    }
}
