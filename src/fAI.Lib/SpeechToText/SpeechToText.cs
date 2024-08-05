using Newtonsoft.Json;
using System;
using DynamicSugar;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace fAI
{

    public partial class SpeechToTextEngine 
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
            var mc = WebClient().AddHeader("Accept", "text/vtt");
            var response = mc.GET(CaptionServiceUrl.TokenReplacer(new { jobId }));
            if (response.Success)
            {
                var text = response.Text;
                return text;
            }
            else throw new SpeechToTextException(response.Exception.Message, response.Exception);
        }

        private string GetTranscriptResult(string jobId)
        {
            var mc = WebClient().AddHeader("Accept", "text/plain");

            var response = mc.GET($"{SpeechToTextServiceUrl}/[JOBID]/transcript".TokenReplacer(new { jobId }));
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
                    {
                        return new SpeechToTextResult { Text = GetTranscriptResult(workId), Captions = execCaption ? GetVTTText(workId) : null };
                    }
                }
                return null;
            });

            //    var timeIncrement = 10;
            //for (var time = 0; time < timeOut; time += timeIncrement)
            //{
            //    Thread.Sleep(timeIncrement * 1000);
            //    var response = WebClient().GET($"{SpeechToTextServiceUrl}/[WORKID]".TokenReplacer(new { workId }));
            //    if (response.Success)
            //    {
            //        var r = SpeechToTextResponse.FromJSON(response.Text);
            //        if (r.Success)
            //        {
            //            var vtt = execCaption ? GetVTTText(workId) : null;
            //            var text = GetTranscriptResult(workId);

            //            return new SpeechToTextResult { Text = text, VTT = vtt };
            //        }
            //    }
            //}
            //throw new SpeechToTextException($"workId:{workId} timed out after {timeOut} seconds");
        }

        public class SourceConfig
        {
            [JsonProperty("url")]
            public string Url { get; set; }
        }

        private SpeechToTextResult ExtractTextFromUrlAudioFile(Uri uri, string languageIsoCode, bool captioning)
        {
            var jsonPayload = new SpeechToTextTranscriptionOptions(languageIsoCode: languageIsoCode, url: uri.ToString()).ToJson();
            var response = WebClient().POST(SpeechToTextServiceUrl, jsonPayload);
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var revAiResponse = SpeechToTextResponse.FromJSON(response.Text);
                var r = WaitForWorkToBeDone(revAiResponse.Id, this._timeOut, captioning);
                r.Language = languageIsoCode;
                RemoveWork(revAiResponse.Id);
                if (string.IsNullOrEmpty(r.Text))
                    throw new SpeechToTextException($"Transciption failed");

                if (captioning && string.IsNullOrEmpty(r.Captions))
                    throw new SpeechToTextException($"Transciption failed, Empty VTT");

                return r;
            }
            else
            {
                var errMsg = response.Exception.Message;
                throw new SpeechToTextException(errMsg);
            }
        }

        public SpeechToTextResult ExtractTextFromFile(string fileName, string languageIsoCode, bool captioning = false)
        {
            if (IsUrl(fileName))
                return ExtractTextFromUrlAudioFile(new Uri(fileName), languageIsoCode, captioning);

            if (!File.Exists(fileName))
                throw new ArgumentException($"File name {fileName} not found");

            var options = new Dictionary<string, string>
            {
                ["options"] = new SpeechToTextTranscriptionOptions(languageIsoCode: languageIsoCode).ToJson()
            };

            var response = WebClient().POST(SpeechToTextServiceUrl,  fileName , options, streamName: "media");
            if (response.Success)
            {
                var revAiResponse = SpeechToTextResponse.FromJSON(response.Text);
                var r = WaitForWorkToBeDone(revAiResponse.Id, this._timeOut, captioning);
                r.Language = languageIsoCode;
                RemoveWork(revAiResponse.Id);
                if (string.IsNullOrEmpty(r.Text))
                    throw new SpeechToTextException($"Transcription failed");
                if (captioning && string.IsNullOrEmpty(r.Captions))
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
