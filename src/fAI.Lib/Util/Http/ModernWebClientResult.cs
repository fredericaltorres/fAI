using System;
using System.Diagnostics;

namespace fAI
{
    public class ModernWebClientResult
    {
        public string Text { get; set; }
        public string ContenType { get; set; }
        public Exception Exception { get; set; }
        public bool Success { get { return this.Exception == null; } }
        public Stopwatch Stopwatch { get; set; }
        public byte[] Buffer;

        public ModernWebClientResult()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }


        public void SetException(Exception ex)
        {
            Stopwatch.Stop();
            this.Exception = ex;
        }
        public void SetResult(byte[] buffer , string contentType)
        {
            Stopwatch.Stop();
            this.ContenType = contentType;
            this.Buffer = buffer;
        }

        public string SetText(byte[] buffer, string contentType)
        {
            Stopwatch.Stop();
            this.Text = System.Text.Encoding.UTF8.GetString(buffer);
            this.ContenType = contentType;
            return this.Text;
        }
        public string SetText(string text, string contentType)
        {
            Stopwatch.Stop();
            this.Text = text;
            this.ContenType = contentType;
            return this.Text;
        }
    }
}

