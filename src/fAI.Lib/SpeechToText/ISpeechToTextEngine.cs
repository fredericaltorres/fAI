namespace fAI
{
    public interface ISpeechToTextEngine
    {
        SpeechToTextResult ExtractText(string fileNameOrUrl, string languageIsoCode, bool extractCaptions);
    }
}
