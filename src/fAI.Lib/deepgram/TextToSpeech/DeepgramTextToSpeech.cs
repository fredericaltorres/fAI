using Deepgram;
using Deepgram.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace fAI
{

    /// <summary>
    /// https://developers.deepgram.com/docs/text-to-speech
    /// https://github.com/deepgram-devs/code-samples
    /// NOTES https://github.com/deepgram-starters/text-to-speech-starter-node
    /// </summary>
    public class DeepgramTextToSpeech
    {
        private readonly DeepgramClient _deepgramClient;

        public DeepgramTextToSpeech(DeepgramClient deepgramClient)
        {
            this._deepgramClient = deepgramClient;
        }
       
        public const string DEFAULT_ENGLISH_MODEL = "aura-asteria-en";
        public const string DEFAULT_VOICE_NAME_EN_US = "Orpheus";

        

        //
        public async Task CreateAsync(string text, string filePath, 
            string voiceName = DEFAULT_VOICE_NAME_EN_US,
            string model = DEFAULT_ENGLISH_MODEL,
            string encoding = "mp3",
            int bitRate = 32000,
            int sampleRate= 24000 // https://developers.deepgram.com/docs/tts-media-output-settings#audio-format-combinations
            )
        {
            text = text.Replace(Environment.NewLine, " ").Replace("\r", " ").Replace("\n", " ");
            text = text.Replace(@"""",@"\""");

            string json = $"{{\"Text\": \"{text}\"}}";
            string url = $"https://api.deepgram.com/v1/speak?Model={model}&encoding={encoding}&bit_rate={bitRate}";
            string apiKey = DeepgramAI.GetKey();

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    httpClient.DefaultRequestHeaders.Add("Authorization", "token " + apiKey);
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        using (Stream audioStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (FileStream fileStream = File.Create(filePath))
                            {
                                using (BinaryWriter writer = new BinaryWriter(fileStream))
                                {
                                    // Copy the binary data from the response stream to the file stream
                                    byte[] buffer = new byte[8192];
                                    int bytesRead;
                                    while ((bytesRead = await audioStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                    {
                                        writer.Write(buffer, 0, bytesRead);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                         throw new ApplicationException($"Error: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Error: {ex.Message}", ex);
                }
            }
        }
    }
}
