using System;
using System.Collections.Generic;

namespace fAI
{
    public class Anthropic : Logger
    {
        public static string AnthropicApiVersion = "2023-06-01" ;

        public Anthropic(int timeOut = -1)
        {
            HttpBase._key = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;
        }
        
        public AnthropicCompletions _completions = null;
        public AnthropicCompletions Completions => _completions ?? (_completions = new AnthropicCompletions());
    }

    public class Mistral : Logger
    {

        public Mistral(int timeOut = -1)
        {
            HttpBase._key = Environment.GetEnvironmentVariable("MISTRAL_API_KEY");
            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;
        }

        public MistralCompletions _completions = null;
        public MistralCompletions Completions => _completions ?? (_completions = new MistralCompletions());
    }
}
