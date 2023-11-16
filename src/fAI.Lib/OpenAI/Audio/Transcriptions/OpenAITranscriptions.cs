using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Cache;

namespace fAI
{
    public class OpenAITranscriptions : OpenAIHttpBase
    {
        const string __url = "https://api.openai.com/v1/audio/transcriptions";

        public OpenAITranscriptions(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut, openAiKey, openAiOrg)
        {
        }

        public string Create(string audioFile, string model = "whisper-1")
        {
            var wc = InitWebClient();
            using (var fileStream = File.OpenRead(audioFile))
            {
                var properties = new Dictionary<string, string>() 
                {
                     ["model"] = model,
                };
                var response = wc.POST(__url, fileStream, properties);
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

