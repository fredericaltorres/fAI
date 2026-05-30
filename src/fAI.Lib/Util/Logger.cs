using DynamicSugar;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace fAI
{
    public class MeasureTime : IDisposable
    {
        public string _methodName { get; }
        Stopwatch _stopwatch;
        private readonly string _parameter1;
        private readonly string _parameter2;

        public static bool _traceOn = true;

        public MeasureTime(string parameter1 = null, string parameter2 = null, [CallerMemberName] string methodName = null)
        {
            this._parameter1 = parameter1;
            this._parameter2 = parameter2;
            _methodName = methodName;
            if (_traceOn)
            {
                HttpBase.Trace($"[{_methodName}.Start] {_parameter1}, {_parameter2}", this, _methodName);
            }
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            var parameterStr = string.IsNullOrEmpty(_parameter1) ? "" : $" Parameter: {_parameter1}, ";
            if (_traceOn)
            {
                HttpBase.Trace($"[{_methodName}.End] {parameterStr}Duration : {_stopwatch.ElapsedMilliseconds / 1000.0:0.0} s", this, _methodName);
            }
        }
    }

    public class Logger
    {
        public static bool TraceOn { get; set; } = true;
        public static bool TraceToConsole { get; set; } = false;

        public static string DefaultLogFileName = @"c:\temp\fAI.log";
        public static string LogFileName = null;

        private static void TraceToFile(string message, int recursiveIndex = 0)
        {
            if (LogFileName == null)
                LogFileName = Environment.GetEnvironmentVariable("FAI_LOG_FILE");
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

                var m = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}][fAI][{className}{methodName}()]{message}".Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");

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
