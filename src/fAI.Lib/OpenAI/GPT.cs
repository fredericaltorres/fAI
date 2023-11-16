using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;

namespace fAI
{
    public class OpenAI
    {
        public OpenAIAudio Audio { get; private set; } = new OpenAIAudio();
    }

    /// <summary>
    /// https://platform.openai.com/docs/guides/text-to-speech
    /// </summary>
    public class  OpenAIAudio
    {
        public OpenAISpeech Speech { get; private set; } = new OpenAISpeech();
    }


    public class OpenAISpeech : OpenAIHttpBase
    {
        const string __url = "https://api.openai.com/v1/audio/speech";

        public enum Voices
        {
            alloy,
            echo,
            fable,
            onyx,
            nova,
            shimmer
        }
        public OpenAISpeech(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut, openAiKey, openAiOrg)
        {
        }

        private string GetPayLoad(string text, Voices voice, string model)
        {
            return JsonConvert.SerializeObject(new {
                model = model,
                input = text,
                voice =  voice.ToString()
            });
        }

        public string Create(string input, Voices voice, string model = "tts-1")
        {
            var wc = InitWebClient();
            var response = wc.POST(__url, GetPayLoad(input, voice, model));
            if (response.Success)
            {
                var ext = wc.GetResponseImageExtension();
                var tmpMp3FileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".mp3");
                File.WriteAllBytes(tmpMp3FileName, response.Buffer);
                return tmpMp3FileName;
            }
            else throw new OpenAIAudioSpeechException($"{nameof(Create)}() failed - {response.Exception.Message}", response.Exception);
        }
    }

    public class OpenAIHttpBase
    {
        private int _timeout = 60 * 4;

        protected string _openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        protected string _openAiOrg = Environment.GetEnvironmentVariable("OPENAI_ORGANIZATION_ID");

        public OpenAIHttpBase(int timeOut = -1, string openAiKey = null, string openAiOrg = null)
        {
            if (timeOut > 0)
                _timeout = timeOut;

            if (openAiKey != null)
                _openAiKey = openAiKey;

            if (openAiOrg != null)
                _openAiOrg = openAiOrg;
        }

        protected ModernWebClient InitWebClient()
        {
            var mc = new ModernWebClient(_timeout);
            mc.AddHeader("Authorization", $"Bearer {_openAiKey}")
              .AddHeader("OpenAI-Organization", _openAiOrg)
              .AddHeader("Content-Type", "application/json")
              .AddHeader("Accept", "application/json");
            return mc;
        }
    }

    /*
        https://platform.openai.com/docs/api-reference/completions
        https://platform.openai.com/docs/api-reference/chat
     */
    public partial class GPT  : OpenAIHttpBase
    {
        public GPT(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut,  openAiKey, openAiOrg)
        {
        }

        private static int CountWords(string str)
        {
            str = str.Trim();
            if (string.IsNullOrEmpty(str))
                return 0;

            string[] words = str.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }

        const string OpenAi_ModelUrl = "https://api.openai.com/v1/models";

        public Models GetModels()
        {
            var response = InitWebClient().GET(OpenAi_ModelUrl);
            if(response.Success)
            {
                return Models.FromJSON(response.Text);
            }
            else throw new ChatGPTException($"{nameof(GetModels)}() failed - {response.Exception.Message}", response.Exception);
        }

        public CompletionResponse CompletionCreate(GPTPrompt p)
        {
            var response = InitWebClient().POST(p.Url, p.GetPostBody());
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = CompletionResponse.FromJson(response.Text);
                return r;
            }
            else throw new ChatGPTException($"{nameof(Translate)}() failed - {response.Exception.Message}", response.Exception);
        }

        // https://platform.openai.com/docs/guides/gpt
        public CompletionResponse ChatCompletionCreate(GPTPrompt p)
        {
            #if DEBUG
                Thread.Sleep(550); // to avoid rate limiting by OpenAI
            #endif
            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(p.Url, p.GetPostBody());
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                var r = CompletionResponse.FromJson(response.Text);
                r.GPTPrompt = p;
                r.Sw = sw;
                return r;
            }
            else throw new ChatGPTException($"{nameof(Translate)}() failed - {response.Exception.Message}", response.Exception);
        }

        public  string Summarize(string text, TranslationLanguages sourceLangague) 
        {
            var prompt = new Prompt_GPT_35_TurboInstruct 
            {
                Text = text,
                PrePrompt = "Summarize the following text: \n===\n",
                PostPrompt = "\n===\nSummary:\n",
            };

            return CompletionCreate(prompt).Text.Trim();
        }

        public enum GPT_YesNoResponse
        {
            Yes,
            No,
            Unknown
        }

        public GPT_YesNoResponse IsThis(string systemContent, string yesNoQuestion, string dataText, string forceAnswerToYesNo = ", answer only with yes or no?")
        {
            var prompt = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = systemContent },
                    new GPTMessage{ Role =  MessageRole.user, Content = $"{yesNoQuestion}{forceAnswerToYesNo}{Environment.NewLine}{dataText}" }
                },
                Url = "https://api.openai.com/v1/chat/completions"
            };
            var response = this.ChatCompletionCreate(prompt);
            if (response.Success)
            {
                if(response.Text.ToLowerInvariant().Contains(GPT_YesNoResponse.Yes.ToString().ToLower()))
                    return GPT_YesNoResponse.Yes;
                if (response.Text.ToLowerInvariant().Contains(GPT_YesNoResponse.No.ToString().ToLower()))
                    return GPT_YesNoResponse.No;
                return GPT_YesNoResponse.Unknown;
            }
               
            else 
                throw new ChatGPTException($"{nameof(IsThis)}() failed - {response.ErrorMessage}");
        }

        public string Translate(string text, TranslationLanguages sourceLangague, TranslationLanguages targetLanguage)
        {
            var prompt = new Prompt_GPT_35_TurboInstruct
            {
                Text = $"Translate the following {sourceLangague} text to {targetLanguage}: '{text}'",
            };

            return CompletionCreate(prompt).Text.Trim();
        }
    }
}

