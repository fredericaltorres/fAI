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
            this.Exception = ex;
        }

        public void SetException(WebException ex)
        {
            var responseStream = ex.Response.GetResponseStream();
            if (responseStream != null)
            {
                using (var reader = new StreamReader(responseStream))
                    this.ServerErrorInfo = reader.ReadToEnd();
            }
            SetException((Exception)ex);
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

