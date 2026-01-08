using System;
using System.Collections.Generic;

namespace fAI
{
    public class HumeAI : HttpBase
    {
        public HumeAI(int timeOut = -1, string openAiKey = null, string openAiOrg = null)
        {
            base._key = Environment.GetEnvironmentVariable("HUME_API_KEY");
            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;

            if (openAiKey != null)
                base._key = openAiKey;
        }

        HumeAIAudio _audio = null;
        public HumeAIAudio Audio => _audio ?? (_audio = new HumeAIAudio());

        //OpenAICompletionsEx

        //public OpenAICompletionsEx _completionsEx = null;
        //public OpenAICompletionsEx CompletionsEx => _completionsEx ?? (_completionsEx = new OpenAICompletionsEx());

        //public OpenAICompletions _completions = null;
        //public OpenAICompletions Completions => _completions ?? (_completions = new OpenAICompletions());

        //public OpenAIEmbeddings _embeddings = null;
        //public OpenAIEmbeddings Embeddings => _embeddings ?? (_embeddings = new OpenAIEmbeddings());

        //public OpenAIImage _image = null;
        //public OpenAIImage Image => _image ?? (_image = new OpenAIImage());
    }
}
