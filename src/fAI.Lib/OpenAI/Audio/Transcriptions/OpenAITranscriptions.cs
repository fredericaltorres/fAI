using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Cache;
using static System.Net.Mime.MediaTypeNames;

namespace fAI
{
    /// <summary>
    /// https://platform.openai.com/docs/guides/speech-to-text
    /// </summary>
    public class OpenAITranscriptions : OpenAIHttpBase
    {
        const string __url = "https://api.openai.com/v1/audio/transcriptions";

        public OpenAITranscriptions(int timeOut = -1, string openAiKey = null, string openAiOrg = null)
        {
        }

        public class OpenAITranscriptionsResponse
        {
            public string Text { get; set; }

            public static OpenAITranscriptionsResponse FromJson(string text)
            {
                return JsonUtils.FromJSON<OpenAITranscriptionsResponse>(text);
            }
        }

        // https://github.com/sandrohanea/whisper.net/tree/0d1f691b3679c4eb2d97dcebafda1dc1d8439215
        public string Create(string audioFile, string model = "whisper-1", string responseFormat = "text")
        {
            OpenAI.Trace(new { audioFile, model, responseFormat}, this);

            var wc = InitWebClient(addJsonContentType: false);
            using (var fileStream = File.OpenRead(audioFile))
            {
                var properties = new Dictionary<string, string>()
                {
                     ["model"] = model,
                     // ["response_format"] = responseFormat,
                };
                var response = wc.POST(__url, audioFile, properties);
                if (response.Success)
                {
                    return OpenAITranscriptionsResponse.FromJson(response.Text).Text;
                }
                else
                {
                    if (base.IsError(response.Text))
                        response.SetException(base.GetError(response.Text).ToString());
                    throw new OpenAIAudioSpeechException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
                }
            }
        }
    }
}

