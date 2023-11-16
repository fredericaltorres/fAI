using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Cache;
using static System.Net.Mime.MediaTypeNames;

namespace fAI
{
    public class OpenAITranscriptions : OpenAIHttpBase
    {
        const string __url = "https://api.openai.com/v1/audio/transcriptions";

        public OpenAITranscriptions(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut, openAiKey, openAiOrg)
        {
        }

        public string Create(string audioFile, string model = "whisper-1", string responseFormat = "text")
        {
            var wc = InitWebClient();
            using (var fileStream = File.OpenRead(audioFile))
            {
                var properties = new Dictionary<string, string>()
                {
                     ["model"] = model,
                     ["response_format"] = responseFormat,
                };
                var response = wc.POST(__url, fileStream, properties, streamName: "file");
                if(base.IsError(response.Text))
                    response.SetException(base.GetError(response.Text).ToString());

                if (response.Success)
                {
                    response.SetText(response.Buffer, response.ContenType);
                    return response.Text;
                }
                else throw new OpenAIAudioSpeechException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
            }
        }
    }
}

