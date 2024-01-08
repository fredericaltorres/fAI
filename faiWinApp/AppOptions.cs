namespace faiWinApp
{
    public class AppOptions : JsonObject
    {
        public string WorkFolder { get; set; }
        public string GifDelay { get; set; }
        public string Mp4FirstFrameDuration { get; set; }
        public bool GifRepeat { get; set; }
        public bool GifFade1 { get; set; }
        public bool GifFade6 { get; set; }
        public bool GenerateMP4 { get; set; }
        public bool ZoomIn { get; set; }
        public string ZoomInImageCount { get; set; }
        public bool ViewImageAfterWork { get; set; }
    }

}
