using DynamicSugar;
using fAI.Util.Strings;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace fAI
{
    public class GenericAITranscription : HttpBase, IGenericAITranscription
    {
        //https://openrouter.ai/docs/api/api-reference/stt/create-transcription
        public const string __url = "https://openrouter.ai/api/v1/audio/transcriptions";

        public GenericAITranscription(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }

        public List<string> GetModels()
        {
            return DS.List(
                "openai/gpt-transcribe",
                "openai/gpt-4o-mini-transcribe",
                "openai/whisper-large-v3-turbo",

                "qwen/qwen3-asr-1.7b",
                "qwen/qwen3-asr-0.6b",
                "qwen/qwen3-asr-flash-2026-02-10",

                "x-ai/grok-stt-1.0",
                "deepgram/nova-3",

                "mistralai/voxtral-small-24b-2507-stt",
                "mistralai/voxtral-mini-3b-2507",
                "mistralai/voxtral-mini-transcribe",
                
                "nvidia/nemotron-3.5-asr-streaming-multilingual-0.6b"
                );
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum TranscriptionFormat
        {
            wav,
            mp3
        }

        public class TranscriptionResponse
        {
            public string text { get; set; }
            public TranscriptionResponseUsage usage { get; set; }

            public static TranscriptionResponse FromJson(string json) => JsonConvert.DeserializeObject<TranscriptionResponse>(json);
        }

        public class TranscriptionResponseUsage
        {
            public double cost { get; set; }
            public int input_tokens { get; set; }
            public int output_tokens { get; set; }
            public double seconds { get; set; }
            public int total_tokens { get; set; }
        }

        public (string text, GenericAICompletions.GenericAIUsage usage) Create(
            string audioFileName,
            string model = "openai/whisper-large-v3",
            string language = "en"
            )
        {
            var format = Path.GetExtension(audioFileName).ToLower() == ".wav" ? TranscriptionFormat.wav : TranscriptionFormat.mp3;
            var sw = Stopwatch.StartNew();
            OpenAI.Trace(new { model, format, language, audioFileName}, this);
         
            if (base._key == null)
                base._key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

            var wc = InitWebClient();
            var response = wc.POST(__url, GetPayLoad(audioFileName, format, model, language));
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = TranscriptionResponse.FromJson(response.Text);
                sw.Stop();
                Logger.Trace(response.Text, this);
                var usage = new GenericAICompletions.GenericAIUsage(model, "","")
                {
                    InputTokens = r.usage.input_tokens,
                    OutputTokens = r.usage.output_tokens,
                };
                usage.SetDuration(sw);
                OpenAI.Trace($"[STT] Duration: {sw.ElapsedMilliseconds:00000} ms, Cost: {r.usage.cost:0.0000}  Model: {model}", this);

                return (r.text, usage);
            }
            else throw new OpenAIAudioSpeechException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
        }

        private string GetPayLoad(string audioFileName, TranscriptionFormat format, string model, string language)
        {
            return JsonConvert.SerializeObject(new
            {
                input_audio = new {
                    data = FileUtil.FileToBase64(audioFileName),
                    format,
                },
                language,
                model,
            });
        }
    }
}

