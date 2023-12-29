using DynamicSugar;
using Deepgram;
using Deepgram.Clients;
using Deepgram.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace fAI
{
    public class DeepgramAudio
    {
        private  DeepgramClient _deepgramClient;


        DeepgramTranscriptions _transcriptions = null;
        public DeepgramTranscriptions Transcriptions => _transcriptions ?? (_transcriptions = new DeepgramTranscriptions(_deepgramClient));

        public DeepgramAudio(DeepgramClient deepgramClient)
        {
            this._deepgramClient = deepgramClient;
        }
    }

    public class DeepgramAI : Logger
    {
        DeepgramClient _deepgramClient;

        public DeepgramAI(int timeOut = -1, string key = null)
        {
            var credentials = new Credentials(key == null ? Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY") : key);
            _deepgramClient = new DeepgramClient(credentials);
        }

        DeepgramAudio _audio = null;
        public DeepgramAudio Audio => _audio ?? (_audio = new DeepgramAudio(_deepgramClient));
        
    }
}
