using System;
using System.Collections.Generic;
using static fAI.HumeAISpeech;

namespace fAI
{
    public enum GenericAIAudioProvider
    {
        HUME_AI,
        OPEN_AI
    }
    public partial class GenericAIAudio : HttpBase
    {
        public GenericAIAudio(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }

        public string Create(GenericAIAudioProvider provider,
            string input, string voiceName, string mp3FileName = null)
        {
            OpenAI.Trace(new { input, voiceName }, this);
            switch (provider)
            {
                case GenericAIAudioProvider.HUME_AI:
                    var humeClient = new HumeAI(apiKey: base._key);
                    return humeClient.Audio.Speech.Create(input, voiceName, mp3FileName);
                case GenericAIAudioProvider.OPEN_AI:
                    var openAIClient = new OpenAI(apiKey: base._key);
                    return openAIClient.Audio.Speech.Create(input, voiceName, mp3FileName);
                default:
                    throw new Exception($"Audio provider {provider} not supported.");
            }
            return input;
        }

        
    }

    public partial class GenericAIImage : HttpBase
    {
        public GenericAIImage(int timeOut = -1, string apiKey = null) : base(timeOut, apiKey)
        {
        }

        public List<string> GenerateUrl(string prompt, string model = "dall-e-3", int imageCount = 1, OpenAIImage.ImageSize size = OpenAIImage.ImageSize._1024x1024)
        {
            if (model == "dall-e-3")
            {
                var client = new OpenAI(apiKey: base._key);
                var r = client.Image.GenerateDalle(prompt, size: size);
                return r.GetUrls();
            }
            else
            {
                throw new Exception($"Model {model} not supported for image generation.");
            }
        }

        public List<string> GenerateLocalFile(string prompt, string model = "dall-e-3", int imageCount = 1, OpenAIImage.ImageSize size = OpenAIImage.ImageSize._1024x1024)
        {
            if (model == "dall-e-3")
            {
                var client = new OpenAI(apiKey: base._key);
                var r = client.Image.GenerateDalle(prompt, size: size, model: model);
                if (r.Success)
                    return r.DownloadImages();
                else
                    throw new Exception($"Image generation failed: {r.Exception}");
            }
            else if (model == "gpt-5.5" || model == "gpt-5.3" /* fast one*/)
            {
                var client = new OpenAI(apiKey: base._key);
                var r = client.Image.GenerateGpt(prompt, size: size, model: model);
                if (r.Success)
                    return r.DownloadImages();
                else
                    throw new Exception($"Image generation failed: {r.Exception}");
            }
            else
            {
                throw new Exception($"Model {model} not supported for image generation.");
            }
        }
    }
}