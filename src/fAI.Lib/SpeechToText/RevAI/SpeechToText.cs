using Newtonsoft.Json;
using System;
using DynamicSugar;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace fAI
{

    public class SpeechToTextEngine : ISpeechToTextEngine
    {
        const string SpeechToTextServiceUrl = "https://api.rev.ai/speechtotext/v1/jobs";
        const string CaptionServiceUrl = "https://api.rev.ai/speechtotext/v1/jobs/[JOBID]/captions";

        int _timeOut;
        public string _key;

        public SpeechToTextEngine(string key = null, int timeOut = 600) : base()
        {
            _timeOut = timeOut;
            _key = Environment.GetEnvironmentVariable("FAI_SPEECH_TO_TEXT_KEY");
        }

        private void RemoveWork(string jobId)
        {
            var url = $"{SpeechToTextServiceUrl}/{jobId}";
            var response = WebClient().DELETE(url); // https://docs.rev.ai/resources/tutorials/delete-user-files
            if (!response.Success)
                throw new SpeechToTextException(response.Exception.Message, response.Exception);
        }

        private string GetVTTText(string jobId)
        {
            //var mc = WebClient().AddHeader("Accept", "text/vtt");
            var response = WebClient().AddHeader("Accept", "text/vtt").GET(CaptionServiceUrl.TokenReplacer(new { jobId }));
            if (response.Success)
                return response.Text;
            else 
                throw new SpeechToTextException(response.Exception.Message, response.Exception);
        }

        private string GetTranscriptResult(string jobId)
        {
            //var mc = WebClient().AddHeader("Accept", "text/plain");
            var response = WebClient().AddHeader("Accept", "text/plain").GET($"{SpeechToTextServiceUrl}/[JOBID]/transcript".TokenReplacer(new { jobId }));
            if (response.Success)
            {
                var text = response.Text;
                var tag = "00:00:00";
                var index = text.IndexOf(tag);
                if(index > 0)
                    text = text.Substring(index + tag.Length).Trim();

                return text;
            }
            else throw new SpeechToTextException(response.Exception.Message, response.Exception);
        }

        private SpeechToTextResult WaitForWorkToBeDone(string workId, int timeOut, bool execCaption)
        {
            return Managers.TimeOutManager<SpeechToTextResult>("Test", timeOut, () => {

                var response = WebClient().GET($"{SpeechToTextServiceUrl}/[WORKID]".TokenReplacer(new { workId }));
                if (response.Success)
                {
                    var r = SpeechToTextResponse.FromJSON(response.Text);
                    if (r.Success)
                        return new SpeechToTextResult { Text = GetTranscriptResult(workId), Captions = execCaption ? GetVTTText(workId) : null };
                }
                return null;
            });
        }

        public class SourceConfig
        {
            [JsonProperty("url")]
            public string Url { get; set; }
        }

        private SpeechToTextResult ExtractText(Uri uri, string languageIsoCode, bool extractCaptions)
        {
            var jsonPayload = new SpeechToTextTranscriptionOptions(languageIsoCode: languageIsoCode, url: uri.ToString()).ToJson();
            var response = WebClient().POST(SpeechToTextServiceUrl, jsonPayload);
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var revAiResponse = SpeechToTextResponse.FromJSON(response.Text);
                var r = WaitForWorkToBeDone(revAiResponse.Id, this._timeOut, extractCaptions);
                r.Language = languageIsoCode;
                RemoveWork(revAiResponse.Id);
                if (string.IsNullOrEmpty(r.Text))
                    throw new SpeechToTextException($"Transciption failed");

                if (extractCaptions && string.IsNullOrEmpty(r.Captions))
                    throw new SpeechToTextException($"Transciption failed, Empty VTT");

                return r;
            }
            else
            {
                var errMsg = response.Exception.Message;
                throw new SpeechToTextException(errMsg);
            }
        }

        public SpeechToTextResult ExtractText(string fileNameOrUrl, string languageIsoCode, bool extractCaptions)
        {
            if (IsUrl(fileNameOrUrl))
                return ExtractText(new Uri(fileNameOrUrl), languageIsoCode, extractCaptions);

            if (!File.Exists(fileNameOrUrl))
                throw new ArgumentException($"File name {fileNameOrUrl} not found");

            var options = new Dictionary<string, string>
            {
                ["options"] = new SpeechToTextTranscriptionOptions(languageIsoCode: languageIsoCode).ToJson()
            };

            var response = WebClient().POST(SpeechToTextServiceUrl,  fileNameOrUrl , options, streamName: "media");
            if (response.Success)
            {
                var revAiResponse = SpeechToTextResponse.FromJSON(response.Text);
                var r = WaitForWorkToBeDone(revAiResponse.Id, this._timeOut, extractCaptions);
                r.Language = languageIsoCode;
                RemoveWork(revAiResponse.Id);
                if (string.IsNullOrEmpty(r.Text))
                    throw new SpeechToTextException($"Transcription failed");
                if (extractCaptions && string.IsNullOrEmpty(r.Captions))
                    throw new SpeechToTextException($"Transcription failed, empty vtt response");

                return r;
            }
            else return new SpeechToTextResult { Exception = response.Exception, Language = languageIsoCode };
        }

        public static bool IsUrl(string fileName)
        {
            return fileName.ToLowerInvariant().StartsWith("https://");
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
