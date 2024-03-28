using System.Diagnostics;

namespace fAI
{
    public class DeepgramTranscriptionResult
    {
        public string Text { get; set; }
        public string VTT { get; set; }
        public double Duration => _stopwatch.Elapsed.TotalSeconds;
        internal Stopwatch _stopwatch { get; set; } = new Stopwatch();
    }
}
