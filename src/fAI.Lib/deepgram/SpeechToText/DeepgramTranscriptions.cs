using Deepgram;
using Deepgram.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Cache;
using System.Runtime.InteropServices.ComTypes;
using static System.Net.Mime.MediaTypeNames;

namespace fAI
{
    public class DeepgramTranscriptionResult
    {
        public string Text { get; set; }
        public string VTT { get; set; }
    }
    /// <summary>
    /// https://platform.openai.com/docs/guides/speech-to-text
    /// </summary>
    public class DeepgramTranscriptions
    {
        private readonly DeepgramClient _deepgramClient;

        public DeepgramTranscriptions(DeepgramClient deepgramClient)
        {
            this._deepgramClient = deepgramClient;
        }

        // https://playground.deepgram.com/?smart_format=true&language=en&model=nova-2
        public DeepgramTranscriptionResult Create(string audioFile, string language = "en", bool vtt = false, string model = "nova-2")
        {
            var r = new DeepgramTranscriptionResult();
            DeepgramAI.Trace(new { audioFile, model }, this);

            var options = new PrerecordedTranscriptionOptions()
            {
                Model = model,
                Language = language,
                SmartFormat = true,
                Utterances = vtt,
            };

            var mimeType = GetMimeType(audioFile);

            using (FileStream fs = File.OpenRead(audioFile))
            {
                var stream = new StreamSource(fs, mimeType);
                var response = _deepgramClient.Transcription.Prerecorded.GetTranscriptionAsync(stream, options).GetAwaiter().GetResult();
                r.Text = response.Results.Channels[0].Alternatives[0].Transcript;
                if(vtt)
                    r.VTT = response.ToWebVTT();
            }

            return r;
        }

        private static string GetMimeType(string audioFile)
        {
            string mimeType;
            switch (Path.GetExtension(audioFile).ToLower())
            {
                case ".mp3":
                    mimeType = "audio/mp3";
                    break;
                case ".wav":
                    mimeType = "audio/wav";
                    break;
                case ".ogg":
                    mimeType = "audio/ogg";
                    break;
                case ".flac":
                    mimeType = "audio/flac";
                    break;
                default:
                    throw new Exception($"Unsupported audio file extension: {Path.GetExtension(audioFile)}");
            }

            return mimeType;
        }
    }
}
