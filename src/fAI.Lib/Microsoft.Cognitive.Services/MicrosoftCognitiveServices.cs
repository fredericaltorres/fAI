using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.IO;

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

    public enum AudioFileType
    {
        Wav,
        Mp3
    }

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

        public  void CreateAudioFile(string text, string voiceName, string filename)
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

