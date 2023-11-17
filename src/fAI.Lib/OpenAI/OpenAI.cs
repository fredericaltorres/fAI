using System;

namespace fAI
{
    public class OpenAI 
    {
        public OpenAI(int timeOut = -1, string openAiKey = null, string openAiOrg = null)
        {
            OpenAIHttpBase._openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            OpenAIHttpBase._openAiOrg = Environment.GetEnvironmentVariable("OPENAI_ORGANIZATION_ID");
            OpenAIHttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                OpenAIHttpBase._timeout = timeOut;

            if (openAiKey != null)
                OpenAIHttpBase._openAiKey = openAiKey;

            if (openAiOrg != null)
                OpenAIHttpBase. _openAiOrg = openAiOrg;
        }

        OpenAIAudio _audio = null;

        public OpenAIAudio Audio => _audio ?? (_audio = new OpenAIAudio());


        public OpenAICompletions _completions = null;

        public OpenAICompletions Completions => _completions ?? (_completions = new OpenAICompletions());
    }
}

