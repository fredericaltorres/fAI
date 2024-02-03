using System;
using System.Collections.Generic;

namespace fAI
{
    public class OpenAI : Logger
    {
        public static IReadOnlyList<float> GenerateEmbeddings(string text)
        {
            var client = new OpenAI();
            var r = client.Embeddings.Create(text);
            return r.Data[0].Embedding;
        }

        public OpenAI(int timeOut = -1, string openAiKey = null, string openAiOrg = null)
        {
            HttpBase._key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            HttpBase._openAiOrg = Environment.GetEnvironmentVariable("OPENAI_ORGANIZATION_ID");
            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;

            if (openAiKey != null)
                HttpBase._key = openAiKey;

            if (openAiOrg != null)
                HttpBase. _openAiOrg = openAiOrg;
        }
        
        OpenAIAudio _audio = null;
        public OpenAIAudio Audio => _audio ?? (_audio = new OpenAIAudio());

        public OpenAICompletions _completions = null;
        public OpenAICompletions Completions => _completions ?? (_completions = new OpenAICompletions());

        public OpenAIEmbeddings _embeddings = null;
        public OpenAIEmbeddings Embeddings => _embeddings ?? (_embeddings = new OpenAIEmbeddings());

        public OpenAIImage _image = null;
        public OpenAIImage Image => _image ?? (_image = new OpenAIImage());
    }
}
