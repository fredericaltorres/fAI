using DynamicSugar;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace fAI
{
    public class Logger
    {
        public static bool TraceOn { get; set; } = true;
        public static bool TraceToConsole { get; set; } = false;

        public static string DefaultLogFileName = @"c:\temp\fAI.log";
        public static string LogFileName = null;

        private static void TraceToFile(string message, int recursiveIndex = 0)
        {
            if (LogFileName == null)
                LogFileName = Environment.GetEnvironmentVariable("OPENAI_LOG_FILE");
            if (LogFileName == null)
                LogFileName = DefaultLogFileName;

            try
            {
                File.AppendAllText(LogFileName, message + Environment.NewLine);
            }
            catch (Exception)
            {
                if (recursiveIndex == 0)
                {
                    Thread.Sleep(256);
                    TraceToFile(message, recursiveIndex + 1);
                }
                else throw;
            }
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

                var m = $"{DateTime.Now}|[{className}{methodName}()]{message}".Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");

                if (TraceToConsole)
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

            var s = sb.ToString();
            s = s.Replace(Environment.NewLine, "");
            s = s.Replace("\n", "");
            s = s.Replace("\r", "");
            return Trace(s, This, methodName);
        }

        public static Exception Trace(Exception ex)
        {
            var e = ex as Exception;
            TraceError(e.Message, e);
            return e;
        }
    }
}
