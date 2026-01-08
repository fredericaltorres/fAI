using DynamicSugar;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fAI
{
    /*
        https://platform.openai.com/docs/api-reference/completions
        https://platform.openai.com/docs/api-reference/chat
        https://platform.openai.com/docs/quickstart?context=python
     */

    public partial class OpenAIEmbeddings : HttpBase
    {
        public OpenAIEmbeddings(int timeOut = -1, string openAiKey = null) : base(timeOut, openAiKey)
        {
        }

        const string __url = "https://api.openai.com/v1/embeddings";


        public const string EmbeddingAda002 = "text-embedding-ada-002";
        public const int EmbeddingAda002Dimension = 1536;

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
        
        public EmbeddingResponse Create(string input, string model = EmbeddingAda002)
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

        public List<EmbeddingResponse> CreateBatch(IEnumerable<string> inputs, string model = EmbeddingAda002, int maxBatchSize = 6)
        {
            try
            {
                var semaphore = new SemaphoreSlim(maxBatchSize); // Limit to 6 concurrent calls
                var tasks = new List<Task<EmbeddingResponse>>();

                Logger.TraceOn = false;

                foreach (var input in inputs)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            return Create(input, model);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }

                var results = Task.WhenAll(tasks).GetAwaiter().GetResult();
                return results.ToList();
            }
            finally
            {
                Logger.TraceOn = true;
            }
        }
    }
}

