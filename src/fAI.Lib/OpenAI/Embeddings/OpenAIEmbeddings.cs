using DynamicSugar;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;

namespace fAI
{
    /*
        https://platform.openai.com/docs/api-reference/completions
        https://platform.openai.com/docs/api-reference/chat
        https://platform.openai.com/docs/quickstart?context=python
     */

    public partial class OpenAIEmbeddings : HttpBase
    {
        public OpenAIEmbeddings(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut, openAiKey, openAiOrg)
        {
        }

        const string __url = "https://api.openai.com/v1/embeddings";


        const int MaxTextLength = 4096;

        // Break down text in string of 4096 characters
        public List<string> BreakDownLongTextBasedOnDot(string text)
        {
            var phrases = text.Split('.').ToList().TrimEnd().TrimStart();
            var currentText = new StringBuilder();
            var r = new List<string>(); 
            foreach(var s in phrases)
            {
                currentText.Append(s).Append(".\n");
                if(currentText.Length > MaxTextLength)
                {
                    r.Add(currentText.ToString());
                    currentText.Clear();
                }
            }
            if(currentText.Length > 0)
                r.Add(currentText.ToString());
            return r;
        }
        
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
                r.Text = input; 
                r.Stopwatch = sw;
                return r;
            }
            else throw new ChatGPTException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
        }
    }
}

