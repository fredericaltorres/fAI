using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace fAI
{
    public class ModernWebClientResult
    {
        public string Text { get; set; }
        public string ContenType { get; set; }
        public Exception Exception { get; set; }
        public string ServerErrorInfo { get; set; }

        public bool Success { get { return this.Exception == null; } }
        private Stopwatch _stopwatch { get; set; }
        public byte[] Buffer;

        public ModernWebClientResult()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }


        private static string GetWebExceptionJsonErrorObject(Exception ex)
        {
            if (ex is WebException)
            {
                var wex = ex as WebException;
                using (StreamReader rr = new StreamReader(wex.Response.GetResponseStream()))
                {
                    return rr.ReadToEnd();
                }
            }
            return null;
        }

        public void SetException(string errorMessage)
        {
            if (_stopwatch.IsRunning)
                _stopwatch.Stop();
            this.Exception = new Exception(errorMessage);
        }

        public void SetException(Exception ex)
        {
            if (_stopwatch.IsRunning)
                _stopwatch.Stop();

            this.Text = GetWebExceptionJsonErrorObject(ex);
            this.Exception = ex;
        }

        public void SetResult(byte[] buffer , string contentType)
        {
            if (_stopwatch.IsRunning)
                _stopwatch.Stop();
            this.ContenType = contentType;
            this.Buffer = buffer;
        }

        public string SetText(byte[] buffer, string contentType)
        {
            if (_stopwatch.IsRunning)
                _stopwatch.Stop();
            this.Text = System.Text.Encoding.UTF8.GetString(buffer);
            this.ContenType = contentType;
            return this.Text;
        }
        public string SetText(string text, string contentType)
        {
            if(_stopwatch.IsRunning)
                _stopwatch.Stop();
            this.Text = text;
            this.ContenType = contentType;
            return this.Text;
        }

      
    }
}

