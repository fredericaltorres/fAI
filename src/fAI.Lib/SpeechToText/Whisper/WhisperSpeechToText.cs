using Newtonsoft.Json;
using System;
using DynamicSugar;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;

namespace fAI
{

    // https://github.com/sandrohanea/whisper.net
    // https://platform.openai.com/docs/guides/speech-to-text
    public class WhisperSpeechToText : ISpeechToTextEngine
    {
        int _timeOut;
        public string _key;

        public WhisperSpeechToText(string key = null, int timeOut = 600) : base()
        {
            _timeOut = timeOut;
            _key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (key != null)
                _key = key;
        }

        const string SpeechToTextServiceUrl = "https://api.openai.com/v1/audio/transcriptions";

        // Special Whisper feature
        public bool WordTimestampGranularities { get; set;} = false;

        public SpeechToTextResult ExtractText(string fileNameOrUrl, string languageIsoCode, bool extractCaptions, string model = null)
        {
            model = model == null ? "whisper-1" : model;

            using (var tfh = new TestFileHelper())
            {
                if (SpeechToTextEngine.IsUrl(fileNameOrUrl))
                {
                    var tmpLocalAudioFile = tfh.GetTempFileName("mp3");
                    DownloadMp3(fileNameOrUrl, tmpLocalAudioFile);
                    fileNameOrUrl = tmpLocalAudioFile;
                }

                if (!File.Exists(fileNameOrUrl))
                    throw new ArgumentException($"File name {fileNameOrUrl} not found");

                var options = new Dictionary<string, string> { ["Model"] = model };

                // With Whisper, you get or the text or the VTT but not both
                if (extractCaptions)
                    options["response_format"] = "vtt";

                if (WordTimestampGranularities)
                {
                    options["response_format"] = "verbose_json";
                    options["timestamp_granularities[]"] = "word";
                }

                var response = WebClient().POST(SpeechToTextServiceUrl, fileNameOrUrl, options);
                if (response.Success)
                {
                    if (extractCaptions)
                    {
                        return new SpeechToTextResult()
                        {
                            Language = languageIsoCode,
                            Captions = response.Text
                        };
                    }
                    else
                    {
                        var typedResponse = WhipserSpeechToTextResponse.FromJSON(response.Text);
                        return new SpeechToTextResult()
                        {
                            Text = typedResponse.Text,
                            Language = languageIsoCode,
                        };
                    }
                }
                else return new SpeechToTextResult { Exception = response.Exception, Language = languageIsoCode };
            }
        }

        static void DownloadMp3(string url, string fileName)
        {
            using (var client = new WebClient())
            {
                var buffer = client.DownloadData(url);
                File.WriteAllBytes(fileName, buffer);
            }
        }

        protected ModernWebClient WebClient(bool addJsonContentType = true)
        {
            var mc = new ModernWebClient(_timeOut);
            mc.AddHeader("Authorization", $"Bearer {_key}");

            if (addJsonContentType)
                mc.AddHeader("Content-Type", "application/json")
                  .AddHeader("Accept", "application/json");
            return mc;
        }
    }
}
