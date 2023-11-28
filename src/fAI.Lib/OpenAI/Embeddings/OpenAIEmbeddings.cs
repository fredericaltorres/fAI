using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

namespace fAI
{
    /*
        https://platform.openai.com/docs/api-reference/completions
        https://platform.openai.com/docs/api-reference/chat
        https://platform.openai.com/docs/quickstart?context=python
     */

    public partial class OpenAIEmbeddings : OpenAIHttpBase
    {
        public OpenAIEmbeddings(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut, openAiKey, openAiOrg)
        {
        }

        const string __url = "https://api.openai.com/v1/embeddings";

        public EmbeddingResponse Create(string input, string model= "text-embedding-ada-002")
        {
            var sw = Stopwatch.StartNew();
            var body = new { input, model };
            var response = InitWebClient().POST(__url, JsonConvert.SerializeObject(body));
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = EmbeddingResponse.FromJson(response.Text);
                r.Stopwatch = sw;
                return r;
            }
            else throw new ChatGPTException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
        }
    }
}

