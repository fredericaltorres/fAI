using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace fAI
{
    public class GenericAISpeech : HttpBase, IGenericAISpeech
    {
        //https://openrouter.ai/docs/api/api-reference/tts/create-speech
        public const string __url = "https://openrouter.ai/api/v1/audio/speech";

        public GenericAISpeech(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }
        public class TTSRequest
        {

            public bool OpenRouterSupported { get; set; } = true;
            public List<string> Voices { get; set; }
            public string TestVoice => Voices != null && Voices.Count > 0 ? Voices[0] : null;

            public List<string> TestsVoices
            {
                get
                {
                    if (Voices != null && Voices.Count > 0)
                        return new List<string>() { Voices[0], Voices[this.Voices.Count - 1] };
                    return new List<string>();
                }
            }
            public string Model { get; set; }
            public float PricePerMillionOfChars { get; set; }

            public float ComputeCost(string text)
            {
                return ComputeCost(text.Length);
            }

            public float ComputeCost(int characteres)
            {
                var totalTokens = characteres ;
                return (totalTokens / 1000000f) * PricePerMillionOfChars;
            }
        }

        public List<TTSRequest> TTSVoiceInfos = new List<TTSRequest>
        {
            new TTSRequest { PricePerMillionOfChars=15.15f, Model = "gpt-4o-mini-tts",                  Voices = new List<string>() { "alloy","ash","ballad","coral","echo","fable","nova","onyx","sage","shimmer","verse" }, OpenRouterSupported = false },/* estimated by chat gpt */
            new TTSRequest { PricePerMillionOfChars=60,     Model = "minimax/speech-2.8-turbo",         Voices = new List<string>() { "Friendly_Person", "Friendly_Person" } }, // https://docs.fish.audio/features/text-to-speech
            new TTSRequest { PricePerMillionOfChars=15,     Model = "fish-audio/s2.1-pro",              Voices = new List<string>() { "b347db033a6549378b48d00acb0d06cd", "9a9cf47702da476aa4629e2506d4a857" } }, // https://docs.fish.audio/features/text-to-speech
            new TTSRequest { PricePerMillionOfChars=16,     Model = "mistralai/voxtral-mini-tts-2603",  Voices = new List<string>() { "en_paul_neutral", "en_paul_sad", "en_paul_happy", "en_paul_frustrated", "en_paul_excited", "en_paul_confident", "en_paul_cheerful", "en_paul_angry", "gb_oliver_neutral", "gb_oliver_sad", "gb_oliver_excited", "gb_oliver_curious", "gb_oliver_confident", "gb_oliver_cheerful", "gb_oliver_angry", "fr_marie_sad",  "fr_marie_happy", "fr_marie_excited", "fr_marie_curious", "fr_marie_angry","fr_marie_neutral", "gb_jane_sarcasm", "gb_jane_confused", "gb_jane_shameful", "gb_jane_sad", "gb_jane_neutral", "gb_jane_jealousy", "gb_jane_frustrated", "gb_jane_curious", "gb_jane_confident" } },
            new TTSRequest { PricePerMillionOfChars=15,     Model = "x-ai/grok-voice-tts-1.0",          Voices = new List<string>() { "eve", "ara", "rex", "sal", "leo" } },
            new TTSRequest { PricePerMillionOfChars=30,     Model = "deepgram/aura-2",                  Voices = new List<string>() { "aura-2-thalia-en", "aura-2-agathe-fr", "aura-2-agustina-es", "aura-2-alvaro-es", "aura-2-ama-ja", "aura-2-amalthea-en", "aura-2-andromeda-en", "aura-2-antonia-es", "aura-2-apollo-en", "aura-2-aquila-es", "aura-2-arcas-en", "aura-2-aries-en", "aura-2-asteria-en", "aura-2-athena-en", "aura-2-atlas-en", "aura-2-aurelia-de", "aura-2-aurora-en", "aura-2-beatrix-nl", "aura-2-callista-en", "aura-2-carina-es", "aura-2-celeste-es", "aura-2-cesare-it", "aura-2-cinzia-it", "aura-2-cora-en", "aura-2-cordelia-en", "aura-2-cornelia-nl", "aura-2-daphne-nl", "aura-2-delia-en", "aura-2-demetra-it", "aura-2-diana-es", "aura-2-dionisio-it", "aura-2-draco-en", "aura-2-ebisu-ja", "aura-2-elara-de", "aura-2-electra-en", "aura-2-elio-it", "aura-2-estrella-es", "aura-2-fabian-de", "aura-2-flavio-it", "aura-2-fujin-ja", "aura-2-gloria-es", "aura-2-harmonia-en", "aura-2-hector-fr", "aura-2-helena-en", "aura-2-hera-en", "aura-2-hermes-en", "aura-2-hestia-nl", "aura-2-hyperion-en", "aura-2-iris-en", "aura-2-izanami-ja", "aura-2-janus-en", "aura-2-javier-es", "aura-2-julius-de", "aura-2-juno-en", "aura-2-jupiter-en", "aura-2-kara-de", "aura-2-lara-de", "aura-2-lars-nl", "aura-2-leda-nl", "aura-2-livia-it", "aura-2-luciano-es", "aura-2-luna-en", "aura-2-maia-it", "aura-2-mars-en", "aura-2-melia-it", "aura-2-minerva-en", "aura-2-neptune-en", "aura-2-nestor-es", "aura-2-odysseus-en", "aura-2-olivia-es", "aura-2-ophelia-en", "aura-2-orion-en", "aura-2-orpheus-en", "aura-2-pandora-en", "aura-2-phoebe-en", "aura-2-pluto-en", "aura-2-rhea-nl", "aura-2-roman-nl", "aura-2-sander-nl", "aura-2-saturn-en", "aura-2-selena-es", "aura-2-selene-en", "aura-2-silvia-es", "aura-2-sirio-es", "aura-2-theia-en", "aura-2-uzume-ja", "aura-2-valerio-es", "aura-2-vesta-en", "aura-2-viktoria-de", "aura-2-zeus-en" } },
            new TTSRequest { PricePerMillionOfChars=15,     Model = "qwen/qwen-audio-3.0-tts-flash",    Voices = new List<string>() { "longanhuan_v3.6", "loongjohn" } },
            new TTSRequest { PricePerMillionOfChars=20,     Model = "qwen/qwen-audio-3.0-tts-plus",     Voices = new List<string>() { "longanlingxin", "longanlufeng" } },
            new TTSRequest { PricePerMillionOfChars=15,     Model = "microsoft/mai-voice-2-flash",      Voices = new List<string>() { "en-US-Harper:MAI-Voice-2", "es-MX-Valeria:MAI-Voice-2", "fr-FR-Soleil:MAI-Voice-2", "de-DE-Klaus:MAI-Voice-2" } },
            new TTSRequest { PricePerMillionOfChars=0,      Model = "deepgram/flux-tts:free",           Voices = new List<string>() { "flux-alexis-en", "flux-bree-en", "flux-brittany-en", "flux-brooke-en", "flux-bruce-en", "flux-cliff-en", "flux-cole-en", "flux-colin-en", "flux-conor-en", "flux-donovan-en", "flux-drew-en", "flux-elise-en", "flux-gemma-en", "flux-haley-en", "flux-hannah-en", "flux-heather-en", "flux-jack-en", "flux-kai-en", "flux-kelsey-en", "flux-kit-en", "flux-maeve-en", "flux-marcelo-en", "flux-marcus-en", "flux-meena-en", "flux-meghan-en", "flux-miles-en", "flux-naveen-en", "flux-paige-en", "flux-priya-en", "flux-rufus-en", "flux-sean-en", "flux-sharon-en", "flux-sienna-en", "flux-tanner-en", "flux-wade-en", "flux-wes-en" } },
            new TTSRequest { PricePerMillionOfChars=0.62f,  Model = "hexgrad/kokoro-82m",               Voices = new List<string>() { "af_alloy", "af_aoede", "af_bella", "af_heart", "af_jessica", "af_kore", "af_nicole", "af_nova", "af_river", "af_sarah", "af_sky", "am_adam", "am_echo", "am_eric", "am_fenrir", "am_liam", "am_michael", "am_onyx", "am_puck", "am_santa", "bf_alice", "bf_emma", "bf_isabella", "bf_lily", "bm_daniel", "bm_fable", "bm_george", "bm_lewis", "ef_dora", "em_alex", "em_santa", "ff_siwis", "hf_alpha", "hf_beta", "hm_omega", "hm_psi", "if_sara", "im_nicola", "jf_alpha", "jf_gongitsune", "jf_nezumi", "jf_tebukuro", "jm_kumo", "pf_dora", "pm_alex", "pm_santa", "zf_xiaobei", "zf_xiaoni", "zf_xiaoxiao", "zf_xiaoyi", "zm_yunjian", "zm_yunxi", "zm_yunxia", "zm_yunyang" } },
        };

        public string Create(
            string input, 
            string voice,
            string model,
            string mp3FileName = null, 
            string instructions = "Speak in a cheerful and positive tone.", 
            int inputTokenCount = -1,
            float cost = 0,
            bool useOpenAI = false)
        {

            var sw = Stopwatch.StartNew();
            OpenAI.Trace(new { input, voice, model }, this);

            if (useOpenAI) // this model "gpt-4o-mini-tts" is not supported by OpenRouter, so we use OpenAI directly for this model
            {
                var openAI = new OpenAI(apiKey: base._key);
                mp3FileName = openAI.Audio.Speech.Create(input, voice, model, mp3FileName, instructions, inputTokenCount, cost);
                sw.Stop();
                var costStr = $"Cost: ${cost:0.00000},";
                OpenAI.Trace($"[TTS] Duration: {sw.ElapsedMilliseconds:00000} ms, {costStr} Model: {model}, mp3FileName: ({mp3FileName})", this);
                return mp3FileName;
            }

            if(base._key == null)
                base._key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

            if (mp3FileName == null)
                mp3FileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".mp3");

            var wc = InitWebClient();
            var response = wc.POST(__url, GetPayLoad(input, voice, model, instructions));
            if (response.Success)
            {
                var ext = wc.GetResponseImageExtension();
                File.WriteAllBytes(mp3FileName, response.Buffer);
                sw.Stop();
                var costStr = $"Cost: ${cost:0.00000},";
                OpenAI.Trace($"[TTS] Duration: {sw.ElapsedMilliseconds:00000} ms, {costStr} Model: {model}, mp3FileName: ({mp3FileName})", this);
                return mp3FileName;
            }
            else throw new OpenAIAudioSpeechException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
        }

        private string GetPayLoad(string input, string voice, string model, string instructions, string response_format = "mp3", float speed = 1f)
        {
            return JsonConvert.SerializeObject(new
            {
                input,
                model,
                response_format,
                speed,
                voice
            });
        }
    }
}

