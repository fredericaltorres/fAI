namespace fAI
{
    public class SpeechToTextResult
    {
        public string Text { get; set; }
        public string Captions { get; set; }
        public string Language { get; set; }

        public System.Exception Exception { get; set; }
        public bool Success => Exception == null;
    }
}
