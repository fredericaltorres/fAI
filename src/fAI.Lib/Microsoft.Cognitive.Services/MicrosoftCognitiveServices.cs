using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fAI
{

    public class MicrosoftCognitiveServices
    {
        SpeechConfig _config;
        
        string _key     = Environment.GetEnvironmentVariable("MICROSOFT_COGNITIVE_SERVICES_KEY");
        string _region  = Environment.GetEnvironmentVariable("MICROSOFT_COGNITIVE_SERVICES_REGION");

        public MicrosoftCognitiveServices(string key = null, string region = "eastus")
        {
            if(key != null)
                _key = key;

            if(region != null)
               _region = region;

            _config = SpeechConfig.FromSubscription(_key, _region);
        }

        private AudioConfig GetAudioConfiguration(string fileName, AudioFileType audioType)
        {
            var audioConfig = AudioConfig.FromWavFileOutput(fileName);
            if (audioType == AudioFileType.Mp3)
                _config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio48Khz96KBitRateMonoMp3); 
            else
                _config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff44100Hz16BitMonoPcm);
            return audioConfig;
        }


        public class STTResult
        {
            internal StringBuilder _text;
            internal StringBuilder _error;
            public bool Succeeded => _error.Length == 0;
            public string Text  => _text.ToString().Trim();
            public string Error => _error.ToString();
        }

        public async Task<STTResult> ExecuteSTT(string audioFileName)
        {
            var sbText = new StringBuilder(1024);
            var sbError = new StringBuilder(1024);
            var r = new STTResult { _text = sbText, _error = sbError };
            var shouldDeleteFile = false;
            var extension = Path.GetExtension(audioFileName).ToLowerInvariant();
            if (extension == ".mp3")
            {
                audioFileName = AudioUtil.ConvertMp3ToWav(audioFileName);
                shouldDeleteFile = true;
            }
            try
            {
                // Set the mode of input language detection to either "AtStart" (the default) or "Continuous".
                // Please refer to the documentation of Language ID for more information.
                // http://aka.ms/speech/lid?pivots=programming-language-csharp
                _config.SetProperty(PropertyId.SpeechServiceConnection_LanguageIdMode, "Continuous");
                var stopRecognition = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

                using (var audioInput = AudioConfig.FromWavFileInput(audioFileName))
                {
                    using (var recognizer = new SpeechRecognizer(this._config, audioInput))
                    {
                        recognizer.Recognized += (s, e) =>
                        {
                            if (e.Result.Reason == ResultReason.RecognizedSpeech)
                            {
                                sbText.Append(e.Result.Text).AppendLine();
                                // var result = AutoDetectSourceLanguageResult.FromResult(e.Result);
                            }
                            else if (e.Result.Reason == ResultReason.NoMatch)
                            {
                                sbError.Append($"NOMATCH: Speech could not be recognized.").AppendLine();
                            }
                        };

                        recognizer.Canceled += (s, e) =>
                        {
                            if (e.Reason == CancellationReason.Error)
                                sbError.Append($"Reason={e.Reason}, ErrorCode={e.ErrorCode}, {e.ErrorDetails}");
                            //else
                            //    sbError.Append($"Reason={e.Reason}");
                            stopRecognition.TrySetResult(0);
                        };
                        recognizer.SessionStarted += (s, e) => { };
                        recognizer.SessionStopped += (s, e) => { stopRecognition.TrySetResult(0); };

                        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
                        Task.WaitAny(new[] { stopRecognition.Task });
                        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                r._error.Append(ex.ToString());
            }
            finally
            {
                if (shouldDeleteFile)
                    this.RemoveFile(audioFileName);
            }
            return r;
        }

        public  void ExecuteTTS(string text, string voiceName, string filename)
        {
            var ssml = GetSSML(text, voiceName);
            var audioFileType = Path.GetExtension(filename).ToLower() == ".mp3" ? AudioFileType.Mp3 : AudioFileType.Wav;  
            __createAudioFileSSML(ssml, voiceName, filename, audioFileType);
        }

        private static string GetSSML(string text, string voiceName)
        {
            return $@"<speak xmlns=""http://www.w3.org/2001/10/synthesis"" xmlns:mstts=""http://www.w3.org/2001/mstts"" xmlns:emo=""http://www.w3.org/2009/10/emotionml"" version=""1.0"" xml:lang=""en-US"">
                            <voice name=""{voiceName}"">{text}</voice>
                    </speak>";
        }

        private void __createAudioFileSSML(string ssml, string voiceName, string fileName, AudioFileType audioFileType)
        {
            var audioType = AudioFileType.Mp3;

            _config.SpeechSynthesisLanguage = "en-us";
            _config.SpeechSynthesisVoiceName = voiceName;

            this.RemoveFile(fileName);

            var audioConfig = GetAudioConfiguration(fileName, audioType);

            using (var synthetizer = new SpeechSynthesizer(_config, audioConfig))
            {
                SpeechSynthesisResult r = synthetizer.SpeakSsmlAsync(ssml).GetAwaiter().GetResult();
                if (r.Reason != ResultReason.SynthesizingAudioCompleted)
                    throw new TextToSpeechProviderException($"Error generating audio from SSML, Reason:{r.Reason}, ssml:{ssml}");
            }
        }

        const string DEFAULT_LANG   = "en";
        const string DEFAULT_LOCAL  = "en-US";

        static VoiceDefinitions _voiceDefinitionsCached;
        static Languages        _languagesCached;

        public Languages Languages
        {
            get
            {
                if (_languagesCached == null)
                    _languagesCached = Languages.Load();

                return _languagesCached;
            }
        }

        /// <summary>
        /// https://learn.microsoft.com/en-us/azure/cognitive-services/speech-service/rest-text-to-speech?tabs=streaming
        /// </summary>
        /// <returns></returns>
        public VoiceDefinitions GetListOfVoices()
        {
            if(_voiceDefinitionsCached == null)
                _voiceDefinitionsCached = new VoiceDefinitions().Load(this._key, this._region);

            return _voiceDefinitionsCached;
        }

        public VoiceDefinitions GetListOfVoicesByLanguage(string language)
        {
            var voices2 = new VoiceDefinitions();
            foreach (var v in GetListOfVoices())
            {
                if(v.Locale.StartsWith($"{language}-"))
                    voices2.Add(v);
            }
            return voices2;

            //var voiceList = GetListOfVoices().Where(q => languages.Any(p => q.Locale.Contains(p))).ToList();
            //var voices = new VoiceDefinitions();
            //voices.Set(voiceList);
            //return voices;
        }

        public bool RemoveFile(string fileName)
        {
            if (!System.IO.File.Exists(fileName))
                return false;

            try
            {
                System.IO.File.Delete(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

