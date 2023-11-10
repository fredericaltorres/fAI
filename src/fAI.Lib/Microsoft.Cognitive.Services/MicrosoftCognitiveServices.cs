using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;

namespace fAI
{
    internal class TextToSpeechProviderException : Exception
    {
        public TextToSpeechProviderException(string message)
            : base(message)
        {
        }
        public TextToSpeechProviderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public enum AudioType
    {
        WAV,
        MP3
    }

    public class MicrosoftCognitiveServices
    {
        SpeechConfig _config;
        
        string _key     = Environment.GetEnvironmentVariable("MICROSOFT_COGNITIVE_SERVICES_KEY");
        string _region  = Environment.GetEnvironmentVariable("MICROSOFT_COGNITIVE_SERVICES_REGION");

        public MicrosoftCognitiveServices(string key = null, string region = "eastus")
        {
            if(_key != null)
                _key = key;

            if(_region != null)
                _region = region;

            _config = SpeechConfig.FromSubscription(_key, _region);
        }

        private AudioConfig GetAudioConfiguration(string fileName, AudioType audioType)
        {
            var audioConfig = AudioConfig.FromWavFileOutput(fileName);
            if (audioType == AudioType.MP3)
                _config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio48Khz96KBitRateMonoMp3); 
            else
                _config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff44100Hz16BitMonoPcm);
            return audioConfig;
        }

        public  void CreateAudioFile(string text, string voiceName, string fileName)
        {
            var ssml = GetSSML(text, voiceName);
            __createAudioFileSSML(ssml, voiceName, fileName);
        }

        private static string GetSSML(string text, string voiceName)
        {
            return $@"<speak xmlns=""http://www.w3.org/2001/10/synthesis"" xmlns:mstts=""http://www.w3.org/2001/mstts"" xmlns:emo=""http://www.w3.org/2009/10/emotionml"" version=""1.0"" xml:lang=""en-US"">
                            <voice name=""{voiceName}"">{text}</voice>
                    </speak>";
        }

        private void __createAudioFileSSML(string ssml, string voiceName, string fileName)
        {
            var audioType = AudioType.MP3;

            _config.SpeechSynthesisLanguage = "en-us";
            _config.SpeechSynthesisVoiceName = voiceName;

            this.RemoveFile(fileName);

            var audioConfig = GetAudioConfiguration(fileName, audioType);

            using (var synthetizer = new SpeechSynthesizer(_config, audioConfig))
            {
                SpeechSynthesisResult r = synthetizer.SpeakTextAsync(ssml).GetAwaiter().GetResult();
                if (r.Reason != ResultReason.SynthesizingAudioCompleted)
                    throw new TextToSpeechProviderException($"Error generating audio from text: {r.Reason}");
            }
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

