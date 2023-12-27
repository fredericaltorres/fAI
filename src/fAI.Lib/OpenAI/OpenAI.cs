using DynamicSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace fAI
{
    public class OpenAI 
    {
        public static IReadOnlyList<float> GenerateEmbeddings(string text)
        {
            var client = new OpenAI();
            var r = client.Embeddings.Create(text);
            return r.Data[0].Embedding;
        }

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

        public OpenAIEmbeddings _embeddings = null;
        public OpenAIEmbeddings Embeddings => _embeddings ?? (_embeddings = new OpenAIEmbeddings());

        public OpenAIImage _image = null;
        public OpenAIImage Image => _image ?? (_image = new OpenAIImage());
        
        public static bool TraceOn { get; set; } = false;

        public const string DefaultLogFileName = @"c:\temp\fAI.log";
        public static string LogFileName = null;

        private static void TraceToFile(string message)
        {
            if(LogFileName == null)
                LogFileName = Environment.GetEnvironmentVariable("OPENAI_LOG_FILE");
            if (LogFileName == null)
                LogFileName = DefaultLogFileName;

            File.AppendAllText(LogFileName, message.Replace(Environment.NewLine,"`r`n") + Environment.NewLine);
        }

        public static string TraceError(string message, object This, [CallerMemberName] string methodName = "")
        {
            return Trace($"[ERROR]{message}", This, methodName);
        }

        public static string Trace(string message, object This, [CallerMemberName] string methodName = "")
        {
            if (TraceOn)
            {
                var className = This.GetType().Name + ".";
                if (className.StartsWith("<"))
                    className = "";

                var m = $"[{DateTime.Now}][{className}{methodName}()]{message}";
                Console.WriteLine(m);
                TraceToFile(m);
            }

            return message;
        }

        public static string Trace(Object poco, object This, [CallerMemberName] string methodName = "")
        {
            var d = ReflectionHelper.GetDictionary(poco);
            var sb = new System.Text.StringBuilder();
            foreach (var k in d.Keys)
                sb.Append($"{k}: {d[k]}, ");

            return Trace(sb.ToString(), This, methodName);
        }
    }
}
