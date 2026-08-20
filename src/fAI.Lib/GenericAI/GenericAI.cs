using DynamicSugar;
using fAI.AnthropicLib;
using fAI.Google;
using fAI.Util.Strings;
using Markdig.Extensions.Tables;
using Mistral.SDK.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpToken;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using static DynamicSugar.DS;
using static fAI.GenericAI;
using static fAI.GoogleAICompletions;
using static fAI.GoogleAICompletions.GoogleAICompletionsResponse;
using static fAI.HumeAISpeech;
using static fAI.OpenAIImage;
using static fAI.SkillManager;
using static System.Net.Mime.MediaTypeNames;

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

        public List<TTSRequest>  TTSVoiceInfos = new List<TTSRequest>
        {
            new TTSRequest { OpenRouterSupported= false,
                PricePerMillionOfChars=15.15f /* estimated by chat gpt */,  
                Model = "gpt-4o-mini-tts", Voices = new List<string>() { "alloy","ash","ballad","coral","echo","fable","nova","onyx","sage","shimmer","verse" } },

            new TTSRequest {  PricePerMillionOfChars=16, Model = "mistralai/voxtral-mini-tts-2603", Voices = new List<string>() {
                "en_paul_neutral", "en_paul_sad", "en_paul_happy", "en_paul_frustrated", "en_paul_excited", "en_paul_confident", "en_paul_cheerful", "en_paul_angry",
                "gb_oliver_neutral", "gb_oliver_sad", "gb_oliver_excited", "gb_oliver_curious", "gb_oliver_confident", "gb_oliver_cheerful", "gb_oliver_angry",
                "fr_marie_sad",  "fr_marie_happy", "fr_marie_excited", "fr_marie_curious", "fr_marie_angry","fr_marie_neutral",
                "gb_jane_sarcasm", "gb_jane_confused", "gb_jane_shameful", "gb_jane_sad", "gb_jane_neutral", "gb_jane_jealousy", "gb_jane_frustrated", "gb_jane_curious", "gb_jane_confident" } },

            new TTSRequest {  PricePerMillionOfChars=15, Model = "x-ai/grok-voice-tts-1.0", Voices = new List<string>() { "eve", "ara", "rex", "sal", "leo" } },
            new TTSRequest {  PricePerMillionOfChars=30, Model = "deepgram/aura-2", Voices = new List<string>() { "aura-2-thalia-en", "aura-2-agathe-fr", "aura-2-agustina-es", "aura-2-alvaro-es", "aura-2-ama-ja", "aura-2-amalthea-en", "aura-2-andromeda-en", "aura-2-antonia-es", "aura-2-apollo-en", "aura-2-aquila-es", "aura-2-arcas-en", "aura-2-aries-en", "aura-2-asteria-en", "aura-2-athena-en", "aura-2-atlas-en", "aura-2-aurelia-de", "aura-2-aurora-en", "aura-2-beatrix-nl", "aura-2-callista-en", "aura-2-carina-es", "aura-2-celeste-es", "aura-2-cesare-it", "aura-2-cinzia-it", "aura-2-cora-en", "aura-2-cordelia-en", "aura-2-cornelia-nl", "aura-2-daphne-nl", "aura-2-delia-en", "aura-2-demetra-it", "aura-2-diana-es", "aura-2-dionisio-it", "aura-2-draco-en", "aura-2-ebisu-ja", "aura-2-elara-de", "aura-2-electra-en", "aura-2-elio-it", "aura-2-estrella-es", "aura-2-fabian-de", "aura-2-flavio-it", "aura-2-fujin-ja", "aura-2-gloria-es", "aura-2-harmonia-en", "aura-2-hector-fr", "aura-2-helena-en", "aura-2-hera-en", "aura-2-hermes-en", "aura-2-hestia-nl", "aura-2-hyperion-en", "aura-2-iris-en", "aura-2-izanami-ja", "aura-2-janus-en", "aura-2-javier-es", "aura-2-julius-de", "aura-2-juno-en", "aura-2-jupiter-en", "aura-2-kara-de", "aura-2-lara-de", "aura-2-lars-nl", "aura-2-leda-nl", "aura-2-livia-it", "aura-2-luciano-es", "aura-2-luna-en", "aura-2-maia-it", "aura-2-mars-en", "aura-2-melia-it", "aura-2-minerva-en", "aura-2-neptune-en", "aura-2-nestor-es", "aura-2-odysseus-en", "aura-2-olivia-es", "aura-2-ophelia-en", "aura-2-orion-en", "aura-2-orpheus-en", "aura-2-pandora-en", "aura-2-phoebe-en", "aura-2-pluto-en", "aura-2-rhea-nl", "aura-2-roman-nl", "aura-2-sander-nl", "aura-2-saturn-en", "aura-2-selena-es", "aura-2-selene-en", "aura-2-silvia-es", "aura-2-sirio-es", "aura-2-theia-en", "aura-2-uzume-ja", "aura-2-valerio-es", "aura-2-vesta-en", "aura-2-viktoria-de", "aura-2-zeus-en" } },

            new TTSRequest {   PricePerMillionOfChars=15, Model = "qwen/qwen-audio-3.0-tts-flash", Voices = new List<string>() { "longanhuan_v3.6", "loongjohn" } },
            new TTSRequest {   PricePerMillionOfChars=20, Model = "qwen/qwen-audio-3.0-tts-plus", Voices = new List<string>() { "longanlingxin", "longanlufeng" } },

            new TTSRequest { PricePerMillionOfChars=15, Model = "microsoft/mai-voice-2-flash", Voices = new List<string>() { "en-US-Harper:MAI-Voice-2", "es-MX-Valeria:MAI-Voice-2", "fr-FR-Soleil:MAI-Voice-2", "de-DE-Klaus:MAI-Voice-2" } },
            new TTSRequest {  PricePerMillionOfChars=0, Model = "deepgram/flux-tts:free", Voices = new List<string>() { "flux-alexis-en", "flux-bree-en", "flux-brittany-en", "flux-brooke-en", "flux-bruce-en", "flux-cliff-en", "flux-cole-en", "flux-colin-en", "flux-conor-en", "flux-donovan-en", "flux-drew-en", "flux-elise-en", "flux-gemma-en", "flux-haley-en", "flux-hannah-en", "flux-heather-en", "flux-jack-en", "flux-kai-en", "flux-kelsey-en", "flux-kit-en", "flux-maeve-en", "flux-marcelo-en", "flux-marcus-en", "flux-meena-en", "flux-meghan-en", "flux-miles-en", "flux-naveen-en", "flux-paige-en", "flux-priya-en", "flux-rufus-en", "flux-sean-en", "flux-sharon-en", "flux-sienna-en", "flux-tanner-en", "flux-wade-en", "flux-wes-en" } },
            //new TTSRequest { Model = "google/gemini-3.1-flash-tts-preview", Voices = new List<string>() {"Zephyr" , "Puck" , "Charon" , "Kore" , "Fenrir" , "Leda" , "Orus" , "Aoede" , "Callirrhoe" , "Autonoe" , "Enceladus" , "Iapetus" , "Umbriel" , "Algieba" , "Despina" , "Erinome" , "Algenib" , "Rasalgethi" , "Laomedeia" , "Achernar" , "Alnilam" , "Schedar" , "Gacrux" , "Pulcherrima" , "Achird" , "Zubenelgenubi" , "Vindemiatrix" , "Sadachbia" , "Sadaltager" , "Sulafat" } },
            new TTSRequest { PricePerMillionOfChars=0.62f,  Model = "hexgrad/kokoro-82m", Voices = new List<string>() { "af_alloy", "af_aoede", "af_bella", "af_heart", "af_jessica", "af_kore", "af_nicole", "af_nova", "af_river", "af_sarah", "af_sky", "am_adam", "am_echo", "am_eric", "am_fenrir", "am_liam", "am_michael", "am_onyx", "am_puck", "am_santa", "bf_alice", "bf_emma", "bf_isabella", "bf_lily", "bm_daniel", "bm_fable", "bm_george", "bm_lewis", "ef_dora", "em_alex", "em_santa", "ff_siwis", "hf_alpha", "hf_beta", "hm_omega", "hm_psi", "if_sara", "im_nicola", "jf_alpha", "jf_gongitsune", "jf_nezumi", "jf_tebukuro", "jm_kumo", "pf_dora", "pm_alex", "pm_santa", "zf_xiaobei", "zf_xiaoni", "zf_xiaoxiao", "zf_xiaoyi", "zm_yunjian", "zm_yunxi", "zm_yunxia", "zm_yunyang" } },
            
            

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

        private string GetPayLoad(string input, string voice, string model, string instructions, string response_format = "mp3", float speed = 1.2f)
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

    public class GenericAI : HttpBase
    {
        public GenericAISpeech GenericAISpeech => new GenericAISpeech(timeOut: HttpBase._timeout, apiKey: base._key);



        public class Contents : List<ContentMessage>
        {
            public List<GPTMessage>  GetOpenAIContents(string systemPrompt)
            {
                var r = new List<GPTMessage>();
                r.Add(new GPTMessage { Role = MessageRole.system, Content = systemPrompt });

                foreach (var c in this)
                {
                    var gptMessage = new GPTMessage
                    {
                        Role = (MessageRole)Enum.Parse(typeof(MessageRole), c.Role),
                        Content = c.Parts[0].Text
                    };
                    r.Add(gptMessage);
                }
                return r;
            }

            public List<AnthropicMessage> GetAnthropicContents()
            {
                var anthropicContents = new List<AnthropicMessage>();
                foreach (var c in this)
                {
                    anthropicContents.Add(new AnthropicMessage {
                        Role = (MessageRole)Enum.Parse(typeof(MessageRole), c.Role),
                        Content = new List<AnthropicContentMessage>()
                        {
                               new AnthropicContentText()
                               {
                                    Text = c.Parts[0].Text,
                                    Type =  AnthropicContentMessageType.text
                               }
                        }
                    });

                }
                return anthropicContents;
            }

            public List<Content> GetGoogleContents()
            {
                // Convert GenericAI.Contents to GoogleAICompletionsBody.Contents
                var googleContents = List<Content>();
                foreach (var c in this)
                {
                    var googleContent = new Content
                    {
                        role = c.Role,
                        parts = new List<Part>() { new Part { text = c.Parts[0].Text } }
                    };
                    googleContents.Add(googleContent);
                }
                return googleContents;
            }
        }

        public class ContentMessagePart
        {
            [JsonProperty("question")]
            public string Text { get; set; }
        }

        public class ContentMessage 
        {
            [JsonProperty("role")]
            public string Role { get; set; }
            public List<ContentMessagePart> Parts { get; set; }
        }

        public static List<AIModel> GetModels(System.Text.RegularExpressions.Regex filter = null)
        {
            var models = new List<AIModel>();
            models.AddRange(OpenRouter.GetModels());
            models.AddRange(Anthropic.GetModels());
            models.AddRange(GoogleAI.GetModels());
            models.AddRange(OpenAI.GetModels());

            if (filter == null)
                return models;
            else
                return models.Where(m => filter.IsMatch(m.Id)).ToList();
        }
            
        public GenericAI(int timeOut = -1, string apiKey = null, string openAiOrg = null)
        {
            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;

            if (apiKey != null)
                base._key = apiKey;
        }

        public GenericAICompletions _completions = null;
        public GenericAICompletions Completions => _completions ?? (_completions = new GenericAICompletions(ApiKey: base._key));

        public GenericAIImage _images = null;
        public GenericAIImage Images => _images ?? (_images = new GenericAIImage(apiKey: base._key));
    }

    public partial class GenericAICompletions : HttpBase 
    {
        public GenericAICompletions(int timeOut = -1, string ApiKey = null) : base(timeOut, ApiKey)
        {
            _key = ApiKey;
        }

        public object CreateAgenticLoop(string userPrompt, string model,
           string systemPrompt = null,
           List<AnthropicTool> tools = null,
           FunctionCallers functionCallers = null)
        {
            if (Anthropic.GetModels().Select(m => m.Id).Contains(model))
            {
                return new Anthropic(key: base._key).Completions.CreateAgenticLoop(userPrompt, model, systemPrompt, tools, functionCallers);
            }
            else if (GoogleAI.GetModels().Select(m => m.Id).Contains(model))
            {
                return new GoogleAI(apiKey: base._key).Completions.CreateAgenticLoop(userPrompt, model, systemPrompt, tools, functionCallers);
            }
            else throw new Exception($"Model {model} not supported for agentic loop.");
        }

        public class GenericAIUsage 
        {
            public float ComputeCost()
            {
                var model = GenericAI.GetModels().FirstOrDefault(m => m.Id == this.Model);
                if(model == null)
                    return 0f;
                return model.ComputeCost(InputTokens, OutputTokens);
            }

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int TTSTokens { get; set; }
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int STTTokens { get; set; }
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int InputTokens { get; set; }
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int OutputTokens { get; set; }
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int Duration { get; set; }

            [JsonIgnore]
            public int TotalTokens => TTSTokens + STTTokens + InputTokens + OutputTokens;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Model { get; set; }

            [JsonIgnore]
            public string Prompt { get; set; }
            [JsonIgnore]
            public string SystemPrompt { get; set; }

            public DateTime StartTime { get; set; }

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public long AudioFileSize { get; set; }

            public void Add(int token)
            {
                this.InputTokens += token;
            }

            public void Add(AnthropicCompletionResponse a)
            {
                this.InputTokens += a.Usage.input_tokens;
                this.OutputTokens += a.Usage.output_tokens;
            }

            public GenericAIUsage(string model, string prompt, string SystemPrompt)
            {
                this.StartTime = DateTime.UtcNow;
                this.Model = model;
                this.Prompt = prompt;
                this.SystemPrompt = SystemPrompt;
            }

            public void SetDuration(Stopwatch sw)
            {
                sw.Stop();
                this.Duration = (int)sw.ElapsedMilliseconds;
            }

            public void Add(GenericAIUsage u)
            {
                if (u == null)
                    return;

                this.InputTokens += u.InputTokens;
                this.OutputTokens += u.OutputTokens;
                this.TTSTokens += u.TTSTokens;
                this.STTTokens += u.STTTokens;
                this.Duration += u.Duration;
                this.AudioFileSize += u.AudioFileSize;
                this.Prompt += u.Prompt;
                this.SystemPrompt += u.SystemPrompt;

                if(string.IsNullOrEmpty(this.Model))
                    this.Model = u.Model;
            }

            public void SetTokenCount( int inputTokens, int outputTokens)
            {
                this.InputTokens = inputTokens;
                this.OutputTokens = outputTokens;
            }
            public override string ToString()
            {
                if(TTSTokens > 0)
                {
                    return $"[TTS.USAGE]Model: {Model}, TTS Tokens: {TTSTokens}";
                }
                if (STTTokens > 0)
                {
                    return $"[STT.USAGE]Model: {Model}, STT Tokens: {STTTokens}, AudioFileSize: {AudioFileSize}";
                }
                if(InputTokens > 0)
                {
                    return $"[LLM.USAGE]Model: {Model}, InputTokens: {InputTokens}, OutputTokens: {OutputTokens}, Duration: {Duration / 1000f:0.000}, StartTime: {StartTime}, PromptLength: {Prompt?.Length ?? 0}, SystemPromptLength: {SystemPrompt?.Length ?? 0}";
                }
                return $"[UNDEFINED.USAGE]Model: {Model}, Duration: {Duration / 1000f:0.000}, StartTime: {StartTime}, PromptLength: {Prompt?.Length ?? 0}, SystemPromptLength: {SystemPrompt?.Length ?? 0}";
            }
        }

        public GenericAIUsage LastUsage { get; set; } = new GenericAIUsage(null, null, null);

        public SkillFile LoadSkill(string skillName, string skillRootFolder)
        {
            var skills = new SkillManager(skillRootFolder);
            var i = skills.GetSkillInfo(skillName);
            return i.LoadSkill();
        }

        

        public (string, GenericAI.Contents, GenericAIUsage) Create(
            string prompt, string systemPrompt, string model, 
            GenericAI.Contents contents = null, int reTryCounter = 0, string skillName = null, string skillRootFolder = null)
        {
            try
            {
                var (result, updatedContents, usage) = __Create(prompt, systemPrompt, model, contents, skillName, skillRootFolder);

                GenericAI.GetModels().Where(m => m.Id == model).ToList().ForEach(m =>
                {
                    var cost = m.ComputeCost(usage.InputTokens, usage.OutputTokens);
                    HttpBase.Trace($"[COST]Model: {model}, InputTokens: {usage.InputTokens}, OutputTokens: {usage.OutputTokens}, Cost: ${cost:0.0000}", this);
                });

                return (result, updatedContents, usage);
            }
            catch (Exception e)
            {
                if(e.Message.Contains("The request timed out") && reTryCounter < 2)
                {
                    Thread.Sleep((reTryCounter+1)*1000);
                    return Create(prompt, systemPrompt, model, contents, reTryCounter + 1);
                }
                else
                {
                    throw e;
                }
            }
        }

        private (string, GenericAI.Contents, GenericAIUsage) __Create(
            string prompt, string systemPrompt, string model, 
            GenericAI.Contents contents = null,  string skillName = null, string skillRootFolder = null)
        {
            var usage = new GenericAIUsage(model, prompt, systemPrompt);
            var orginalModel = model;
            var sw = Stopwatch.StartNew();
            try
            {
                if(skillName != null && skillRootFolder != null)
                {
                    var nl = Environment.NewLine;
                    var skill = LoadSkill(skillName, skillRootFolder);
                    if (skill != null)
                        systemPrompt = $"{nl}{nl}<skill>{nl}{nl}{skill.MarkdownBody}{nl}{nl}</skill>{nl}{nl}" + systemPrompt;
                }

                contents = contents == null ? new GenericAI.Contents() : contents;

                contents.Add(new GenericAI.ContentMessage
                {
                    Role = "user", // A conversation always starts with user message
                    Parts = new List<GenericAI.ContentMessagePart> { new GenericAI.ContentMessagePart { Text = prompt } }
                });

                if (Anthropic.GetModels().Select(m => m.Id).Contains(model))
                {
                    var isAnthpropicFastMode = model.ToLowerInvariant().EndsWith("-fast");
                    model = model.Replace("-fast", "");

                    var p = new Anthropic_Prompt_Generic(model)
                    {
                        System = systemPrompt,
                        Messages = new List<AnthropicMessage>()
                        {
                            new AnthropicMessage { Role = MessageRole.user,
                                Content = DS.List<AnthropicContentMessage>(new AnthropicContentText(prompt))
                            }
                        }
                    };

                    var anthropicContents = contents.GetAnthropicContents();
                    if (anthropicContents.Count > 1)
                    {
                        p.Messages = anthropicContents;
                    }

                    if (string.IsNullOrEmpty(base._key))
                        base._key = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");


                    var headerDictionary = new Dictionary<string, string>();
                    if (isAnthpropicFastMode)
                    {
                        p.Speed = "fast";
                        headerDictionary = new Dictionary<string, string>()
                        {
                            ["anthropic-beta"] = "fast-mode-2026-02-01",
                            //["anthropic-version"] = "2023-06-01"
                        };
                    }

                    var response = new Anthropic(key: base._key).Completions.Create(p, headerDictionary);
                    usage.SetTokenCount(response.Usage.input_tokens, response.Usage.output_tokens);

                    // Update the contents discussion with the answer from the AI
                    var answerContent = response.Content.FirstOrDefault(c => c.IsText);
                    contents.Add(new GenericAI.ContentMessage
                    {
                        Role = response.Role,
                        Parts = new List<GenericAI.ContentMessagePart>
                        {
                            new GenericAI.ContentMessagePart { Text = answerContent.Text }
                        }
                    });

                    return (answerContent.Text, contents, usage);
                }
                else if (GoogleAI.GetModels().Select(m => m.Id).Contains(model))
                {
                    if (string.IsNullOrEmpty(base._key))
                        base._key = Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY");

                    var googleAIClient = new GoogleAI(apiKey: base._key);

                    // Convert GenericAI.Contents to GoogleAICompletionsBody.Contents
                    var googleContents = contents.GetGoogleContents();
                    var p = googleAIClient.Completions.GetPrompt(prompt, systemPrompt, model, googleContents);
                    var url = googleAIClient.Completions.GetUrl(model);

                    var r = googleAIClient.Completions.Create(p, url, model);

                    usage.SetTokenCount(r.usageMetadata.promptTokenCount, r.usageMetadata.candidatesTokenCount);

                    // Update the contents discussion with the answer from the AI
                    var answerContent = r.candidates[0].content;
                    contents.Add(new GenericAI.ContentMessage
                    {
                        Role = answerContent.role,
                        Parts = new List<GenericAI.ContentMessagePart>
                        {
                            new GenericAI.ContentMessagePart { Text = answerContent.parts[0].text }
                        }
                    });

                    return (r.GetText(), contents, usage);
                }

                else if (OpenRouter.GetModels().Select(m => m.Id).Contains(model))
                {
                    if (string.IsNullOrEmpty(base._key))
                        base._key = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");

                    var openRouter = contents.GetOpenAIContents(systemPrompt);
                    var openRouterClient = new OpenRouter(apiKey: base._key);
                    var p = new Prompt_GPT_4
                    {
                        Messages = new List<GPTMessage>()
                        {
                            new GPTMessage { Role = MessageRole.system, Content = systemPrompt },
                            new GPTMessage { Role = MessageRole.user, Content = prompt },
                        },
                        Model = model
                    };

                    if (openRouter.Count > 1)
                    {
                        p.Messages = openRouter;
                    }

                    var response = openRouterClient.Completions.Create(p);
                    if (response.Success)
                    {
                        //```                            
                        // Update the contents discussion with the answer from the AI
                        var answerContent = response.Choices.First().message;
                        contents.Add(new GenericAI.ContentMessage
                        {
                            Role = answerContent.Role.ToString(), // Role are different in Google:model OpenAI:assistant
                            Parts = new List<GenericAI.ContentMessagePart>
                            {
                                new GenericAI.ContentMessagePart { Text = answerContent.Content }
                            }
                        });

                        usage.SetTokenCount(response.Usage.InputTokens, response.Usage.OutputTokens);

                        var responseText = response.Text;
                        return (responseText, contents, usage);
                    }
                    else return (null, contents, usage);
                }

                else if (OpenAI.GetModels().Select(m => m.Id).Contains(model))
                {
                    if (string.IsNullOrEmpty(base._key))
                        base._key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

                    var openAIContents = contents.GetOpenAIContents(systemPrompt);
                    var openAIClient = new OpenAI(apiKey: base._key);
                    var p = new Prompt_GPT_4
                    {
                        Messages = new List<GPTMessage>()
                        {
                            new GPTMessage { Role = MessageRole.system, Content = systemPrompt },
                            new GPTMessage { Role = MessageRole.user, Content = prompt },
                        },
                        Model = model
                    };

                    if (openAIContents.Count > 1)
                    {
                        p.Messages = openAIContents;
                    }

                    var response = openAIClient.Completions.Create(p);
                    if (response.Success)
                    {
//```                            
                        // Update the contents discussion with the answer from the AI
                        var answerContent = response.Choices.First().message;
                        contents.Add(new GenericAI.ContentMessage
                        {
                            Role = answerContent.Role.ToString(), // Role are different in Google:model OpenAI:assistant
                            Parts = new List<GenericAI.ContentMessagePart>
                            {
                                new GenericAI.ContentMessagePart { Text = answerContent.Content }
                            }
                        });

                        usage.SetTokenCount(response.Usage.InputTokens, response.Usage.OutputTokens);

                        var responseText = response.Text;
                        return (responseText, contents, usage);
                    }
                    else return (null, contents, usage);
                }
                return (null, contents, usage);
            }
            finally
            {
                sw.Stop();
                usage.Duration = (int)sw.ElapsedMilliseconds;
                this.LastUsage = usage;
                model = orginalModel; // because of the possible modification of the model variable for Anthropic fast mode, we want to log the original model name.
                OpenAI.Trace(usage.ToString(), this);
            }
        }

        static string __ConvertPdfToMarkdown(string apiKey, string pdfFilePath, string model, string prompt)
        {
            const string ApiUrl = "https://api.anthropic.com/v1/messages";

            // 1. Read and Base64-encode the PDF
            byte[] pdfBytes = File.ReadAllBytes(pdfFilePath);
            string base64Pdf = Convert.ToBase64String(pdfBytes);

            // 2. Build the JSON request body manually using anonymous objects + Newtonsoft
            var requestObject = new
            {
                model = model,
                max_tokens = 8192,
                messages = new[]
                {
                new
                {
                    role    = "user",
                    content = new object[]
                    {
                        new
                        {
                            type   = "document",
                            source = new
                            {
                                type       = "base64",
                                media_type = "application/pdf",
                                data       = base64Pdf
                            }
                        },
                        new
                        {
                            type = "text",
                            text = prompt
                        }
                    }
                }
            }
            };

            string jsonBody = JsonConvert.SerializeObject(requestObject);

            // 3. Send request using HttpWebRequest (.NET 4.0 compatible)
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiUrl);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody);
            request.ContentLength = bodyBytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bodyBytes, 0, bodyBytes.Length);
            }

            // 4. Read the response
            string responseJson;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseJson = reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8))
                    {
                        string errorBody = reader.ReadToEnd();
                        throw new Exception("API error: " + errorBody, ex);
                    }
                }
                throw;
            }

            // 5. Parse and return the Markdown text using Newtonsoft.Json
            JObject parsed = JObject.Parse(responseJson);
            string result = (string)parsed["content"][0]["text"];

            return result ?? string.Empty;
        }

        public string ConvertPdfToMarkdown(
            string pdfFile,
            string anthorpicApiKey = null,
            string prompt = @"
Extract all the text from this PDF and convert it to clean, well-structured Markdown.
Follow these rules:
- Use # for the document title, ## for sections, ### for subsections
- Preserve tables using Markdown table syntax
- Use **bold** and *italic* where appropriate
- Use bullet points or numbered lists for list content
- Preserve code blocks using ``` fences\n
- Output ONLY the Markdown content, no preamble or explanation",
            string model = "claude-haiku-4-5"   
           )
        {
            var sw = Stopwatch.StartNew();
            if (anthorpicApiKey == null)
                anthorpicApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

            var markDownFile = __ConvertPdfToMarkdown(anthorpicApiKey, pdfFile, model, prompt);
            sw.Stop();
            return markDownFile;
        }

        public class TextImprovementResult
        {
            public string Text { get; set; }
            public string OriginalText { get; set; }
            public double Duration { get; set; }
            public GenericAI.Contents Contents { get; set; }
        }

        public TextImprovementResult TextImprovement(
           string text,
           string language,
           string model,
           string systemPrompt = @"
Improve the [language] in more polished and business-friendly way, for the following phrases.
Use the following rules to guide your improvements:
<rules>
- Do NOT use MARKDOWN formatting.
- Insert a new line between paragraphs.
- Do not add new section like 'Subject'.
 - If the following question is part of an email content, add at the end 'Thanks, sincerely'.
 </rules>
 ===================================
            ",
           GenericAI.Contents contents = null, string skillName = null, string skillRootFolder = null
           )
        {
            var sw = Stopwatch.StartNew();
            systemPrompt = systemPrompt.Template(new { language }, "[", "]");
            var (newText, contents2, usage) = Create(text, systemPrompt, model, contents, skillName: skillName, skillRootFolder: skillRootFolder);
            contents = contents2;
            sw.Stop();
            return new TextImprovementResult
            {
                Text = newText,
                OriginalText = text,
                Duration = sw.ElapsedMilliseconds / 1000.0,
                Contents = contents
            };
        }

        public class SummarizationResult
        {
            public string Summary { get; set; }
            public string Text { get; set; }
            public int TextWordCount => CountWords(Text);
            public int SummaryWordCount => CountWords(Summary);
            public double Duration { get; set; }
            public double PercentageSummzarized => TextWordCount == 0 ? 0 : (1.0 - ((double)SummaryWordCount / (double)TextWordCount)) * 100.0;

            public static int CountWords(string text)
            {
                return OpenAIEmbeddings.CountWordS(text);
            }
        }

        public class ConversationResult
        {
            public string Text { get; set; }
            public string Response { get; set; }
            public double Duration { get; set; }
        }

        public ConversationResult Conversation(
           string text,
           string model,
           string systemPrompt = @"
You are an AI assistant with the knowledge of a internet search engine.
Answer the question to the best of your ability.
            "
           )
        {
            var sw = Stopwatch.StartNew();
            var (response, _, usage) = Create(text, systemPrompt, model);
            sw.Stop();
            return new ConversationResult
            {
                Response = response,
                Text = text,
                Duration = sw.ElapsedMilliseconds / 1000.0
            };
        }

        public SummarizationResult Summarize(
           string text,
           string language,
           string model,
           string systemPrompt = @"
Summarize the following [language] text.
Use the following rules to guide your summarization:
<rules>
- Do NOT use MARKDOWN formatting.
- Insert a new line between paragraphs.
- Maintain the context of the text without altering its meaning.
- Keep the bullet points text concise and to the point.
- Use formal language suitable for business communication.
- Ensure that all key information is included in the bullet points Text.
 </rules>
 ===================================
            ",
           int summarizeWordCount = -1
           )
        {
            if(summarizeWordCount<10)
                summarizeWordCount = 10;

            systemPrompt = systemPrompt.Template(new { language }, "[", "]");
            if(summarizeWordCount > 0)
                systemPrompt = systemPrompt.Replace("<rules>", $"<rules>\r\n- Summarize the text in about {summarizeWordCount} words.\r\n");

            var sw  = Stopwatch.StartNew();
            var (summary, _, usage) = Create(text, systemPrompt, model);
            sw.Stop();
            return new SummarizationResult
            {
                Summary = summary,
                Text = text,
                Duration = sw.ElapsedMilliseconds/1000.0
            };
        }


        public class TranslationResult
        {
            public string SourceText { get; set; }
            public string TranslatedText { get; set; }
            public double Duration { get; set; }
            public string Language { get; set; }
            public string destinationLanguage { get; set; }

        }

        public TranslationResult Translate(
           string text,
           string language,
           string destinationLanguage,
           string model,
           string systemPrompt = @"
Translate the following [language] paragraph into [destinationLanguage].
 ===================================
            "//polished and business-friendly 
           )
        {
            systemPrompt = systemPrompt.Template(new { language, destinationLanguage }, "[", "]");
            var sw = Stopwatch.StartNew();
            var (translatedText, _, usage) = Create(text, systemPrompt, model);
            sw.Stop();
            return new TranslationResult
            {
                SourceText = text,
                TranslatedText = translatedText,
                Language = language,
                destinationLanguage = destinationLanguage,
                Duration = sw.ElapsedMilliseconds / 1000.0
            };
        }

        public class GenerateTitleResult
        {
            public string Title { get; set; }
            public double Duration { get; set; }

            public GenericAICompletions.GenericAIUsage Usage { get; set; }
        }

        public GenerateTitleResult GenerateTitle(
           string text,
           string language,
           string model,
           string systemPrompt = @"
Create a short ""Title"" for the following [language] paragraph.
Use the following rules to guide your summarization:
<rules>
- Do NOT use MARKDOWN formatting.
- Do NOT use MARKDOWN section headers.
- Use formal language suitable for business communication.
 </rules>
 ===================================
            "
           )
        {
            systemPrompt = systemPrompt.Template(new { language }, "[", "]");
            var sw = Stopwatch.StartNew();
            var (title, _, usage) = Create(text, systemPrompt, model);
            sw.Stop();
            return new GenerateTitleResult
            {
                Title = title,
                Duration = sw.ElapsedMilliseconds / 1000.0,
                Usage = usage
            };
        }

        public enum PhraseType
        {
            Undefined,
            Question,
            Order,
            Statement
        }

        public class DetermineTheTypeOfPhraseResult
        {
            [JsonProperty("classification")]
            public string Classification { get; set; }
            public static DetermineTheTypeOfPhraseResult FromJson(string json)
            {
                return JsonConvert.DeserializeObject<DetermineTheTypeOfPhraseResult>(json);
            }

            public PhraseType PhraseType
            {
                get
                {
                    if (Enum.TryParse(Classification, out PhraseType result))
                    {
                        return result;
                    }
                    else
                    {
                        throw new Exception($"Unable to parse classification '{Classification}' to PhraseType enum.");
                    }
                }
            }
        }


        const string LIST_OF_VERB_WHICH_INDICATE_QUESTION = @"
what,where,when,who,which,how,why,can,could,should,would,is,are,do,does,did,will,may,might,must,shall,
list,research,find,determine,tell,analyze,analyse,summarize,locate,identify,search,retrieve,discover,uncover,pinpoint,
track-down,investigate,explore,examine,study,review,inspect,probe,audit,evaluate,assess,compare,calculate,
measure,interpret,classify,categorize,rank,prioritize,diagnose,verify,validate,outline,describe,
explain,report,recap,highlight,illustrate,clarify,compile,organize,structure,tabulate,chart,map-out,
enumerate,itemize,decide,conclude,recommend,suggest,predict,forecast,estimate";


        public PhraseType DetermineTheTypeOfPhrase(
           string text,
           string model,
           string systemPrompt = @"
You are a linguistic classifier. 
Your job is to analyze the provided phrase and categorize it into one of the following four categories:

1.  **Question**: The phrase is asking for information.
2.  **Order**: The phrase is an imperative command or request for action.
3.  **Statement**: The phrase is declarative, providing facts, opinions, or descriptions.
4.  **Unknown**: The phrase does not fit into any of the above categories.

You must respond strictly with a JSON object representing your classification. 
The JSON object must have a single key named ""classification"" holding the selected category as a string value. 
Do not include markdown formatting (like ```json) in the output.

If the phrase contains the words ([listOfVerbWhichIndicateQuestion]) and is asking for information, 
classify it as a ""Question"" even if it is not in a traditional question format.

Examples:
Phrase: ""Could you tell me the time?""
Output: {""classification"": ""Question""}

Phrase: ""List all my tasks for the day""
Output: {""classification"": ""Question""}

Phrase: ""Close the door immediately.""
Output: {""classification"": ""Order""}

Phrase: ""It is raining outside.""
Output: {""classification"": ""Statement""}

Phrase: ""[question]""
Output:
            ",
           string listOfVerbWhichIndicateQuestion = LIST_OF_VERB_WHICH_INDICATE_QUESTION

           )
        {

            listOfVerbWhichIndicateQuestion = listOfVerbWhichIndicateQuestion.Replace("\r", "").Replace("\n", "");
            var cacheEntry = $"DetermineTheTypeOfPhrase: {text}";
            var cacheR = AIPromptCache.Instance.GetPromptResponse(cacheEntry);
            if(cacheR != null)
            {
                HttpBase.Trace(new { cacheHit = true, cacheEntry }, this);
                PhraseType phraseType = (PhraseType)Enum.Parse(typeof(PhraseType), cacheR);
                return phraseType;
            }

            var listOfVerbWhichIndicateQuestionAsList = listOfVerbWhichIndicateQuestion.Split(',').Select(v => v.Trim()).ToList();
            //listOfVerbWhichIndicateQuestionAsList.Sort();
            text = text.Trim();

            // Non AI optimization
            var startWithVerbWhichIndicateQuestion = listOfVerbWhichIndicateQuestionAsList.Any(v => text.IndexOf(v+" ", StringComparison.OrdinalIgnoreCase) == 0);
            if (startWithVerbWhichIndicateQuestion|| text.EndsWith("?"))
            {
                AIPromptCache.Instance.Add(cacheEntry, PhraseType.Question.ToString());
                return PhraseType.Question;
            }

            systemPrompt = systemPrompt.Template(new { text, listOfVerbWhichIndicateQuestion }, "[", "]");
            var sw = Stopwatch.StartNew();
            var (json, _, usage) = Create(text, systemPrompt, model);
            sw.Stop();
            var o = DetermineTheTypeOfPhraseResult.FromJson(json);

            AIPromptCache.Instance.Add(cacheEntry, o.PhraseType.ToString());

            return o.PhraseType;
        }

        public string RePhraseQuestionIntoAffirmation(
           string question,
           string model,
           string systemPrompt = @"
Task: Convert the user question into declarative answer templates. 
Change ""my"" to ""your"" and use ""__SOMETHING__"" as the placeholder for the unknown information.

Examples:
Q: ""What is my number one task to do?""
A: ""Your number one task to do is __SOMETHING__.""

Q: ""What is the capital city of France?""
A: ""The capital city of France is SOMETHING.""

Q: ""When is my next scheduled meeting with Sarah?""
A: ""Your next scheduled meeting with Sarah is at SOMETHING.""

Q: ""What is my current checking account balance?""
A: ""Your current checking account balance is SOMETHING.""

Q: ""What is my frequent flyer number for Delta airlines?""
A: ""Your frequent flyer number for Delta airlines is SOMETHING.""

Current Task:
A: [question]
            "
           )
        {
            systemPrompt = systemPrompt.Template(new { question }, "[", "]");
            var sw = Stopwatch.StartNew();
            var (answer, _, usage) = Create(question, systemPrompt, model);
            sw.Stop();
            return StringUtil.SuperTrim(answer);
        }

        public string FixPhrase(
           string phrase,
           string language,
           string model,
           string systemPrompt = @"
Rewrite the provided phrase into natural, grammatically standard [language].
Constraints:
- Provide only the single best corrected version.
- Do not provide any explanations, context, or alternative options.

            ",
           string prompt = @"
Phrase to fix:
""[phrase]""
"
           )
        {

            systemPrompt = systemPrompt.Template(new { language, phrase }, "[", "]");
            prompt = prompt.Template(new { language, phrase }, "[", "]");
            var sw = Stopwatch.StartNew();
            var (answer, _, usage) = Create(prompt, systemPrompt, model);
            sw.Stop();
            return StringUtil.SuperTrim(answer);
        }

        public class GenerateBulletPointResult
        {
            public string Text { get; set; }
            public double Duration { get; set; }
        }

        public GenerateBulletPointResult GenerateBulletPoints(
           int bulletPointCount,
           string text,
           string language,
           string model,
           string systemPrompt = @"
Create [bulletPointCount] bullet points for the following [language] paragraph.
Use the following rules to guide your summarization:
<rules>
- Each bullet point should be concise and to the point.
- Each bullet point should be about 10 words long.
- Use the character '*' at the beginning of each bullet point.
- Do NOT use MARKDOWN formatting.
- Use formal language suitable for business communication.
 </rules>
 ===================================
            "
           )
        {
            systemPrompt = systemPrompt.Template(new { bulletPointCount, language }, "[", "]");
            var sw = Stopwatch.StartNew();
            var (bulletPointsText, _, usage) = Create(text, systemPrompt, model);
            sw.Stop();
            return new GenerateBulletPointResult
            {
                Text = bulletPointsText,
                Duration = sw.ElapsedMilliseconds / 1000.0
            };
        }

        public class AnswerQuestionBasedOnTextResult
        {
            public string Text { get; set; }
            public double Duration { get; set; }
            public string Model { get; set; }
        }

        public AnswerQuestionBasedOnTextResult AnswerQuestionBasedOnFacts(
           string model,
           string question,
           string facts,

           string systemPrompt = @"
Use ONLY the provided article delimited by triple quotes to answer the question below.
""""[facts]"""".

Use the following rules to guide answer the question below.
<rules>
- Do not mention anything outside of the article.
- In the answer, do not reference the article or say 'According to the article' or 'Based on the question provided'.
- Return the answer in the simplest possible terms.
- Return the answer in the following JSON format: { ""answer"": ""[answer here]"" }
- Do not return any MARKDOWN formatting.
- If the answer cannot be found in the article, write """"[not_found]""""
 </rules>
            ",

           string questionPrompt = @"
Use ONLY the provided article delimited by triple quotes to answer the question: [question].",
           string not_found = "Answer not found."
           )
        {
            systemPrompt = systemPrompt.Template(new { not_found, facts }, "[", "]");
            var userPrompt = questionPrompt.Template(new { question }, "[", "]");
            var sw = Stopwatch.StartNew();
            var (jsonAnswer, _, usage) = Create(userPrompt, systemPrompt, model);
            var answer = base.GetJsonObject(jsonAnswer)["answer"].ToString();
            sw.Stop();
            return new AnswerQuestionBasedOnTextResult
            {
                Text = answer,
                Duration = sw.ElapsedMilliseconds / 1000.0,
                Model = model
            };
        }

        public class KeywordsResponse
        {
            public List<string> keywords { get; set; }
        }

        public List<string> ExtractKeywordFromNotes(
           string text,
           string model,
           string systemPrompt = @"
You are an expert NLP assistant specializing in information extraction. 
Your task is to extract the most relevant keywords and key phrases from the provided text.

Rules:
1. Extract between 5 and 10 keywords/phrases.
2. Prioritize nouns, technical terms, and named entities.
3. Avoid generic words like ""text,"" ""information,"" or ""content.""
4. Output the result as a valid JSON object with the key ""keywords"".
            "
           )
        {
            systemPrompt = systemPrompt.Template(new { tutu = 1 }, "[", "]");
            var userText = @"
Text to analyze:
""""""
[text]
""""""

Extracted Keywords:
".Template(new { text = text.Trim() }, "[", "]");

            var (json, _, usage) = Create(userText, systemPrompt, model);
            json = StringUtil.SmartExtractJson(json);
            var r = JsonConvert.DeserializeObject<KeywordsResponse>(json);
            return r.keywords;
        }

        public AIMetaData ExtractMetaDataFromNotes(
           string text,
           string model,
           string systemPrompt = @"
Extract metadata from notes. Return one JSON object with:
- ""people"": array of people mentioned (empty if none)
- ""action_items"": array of implied to-dos (empty if none)
- ""dates_mentioned"": array of dates YYYY-MM-DD (empty if none)
- ""locations"": array of location, town, country, places, street address (empty if none)
- ""topics"": array of 1-3 short topic tags (always at least one)
- ""type"": one of ""observation"", ""task"", ""idea"", ""reference"", ""person_note""
Only extract what's explicitly there.
            "
           )
        {
            systemPrompt = systemPrompt.Template(new { tutu=1 }, "[", "]");

            var (json, _, usage) = Create(text, systemPrompt, model);

            var result = new HttpBase().GetJsonObject(json).ToString();
            ////////NO KEYWORD FOR NOW ///////var keywords = this.ExtractKeywordFromNotes(text, model);

            return new AIMetaData { MetaData = JObjectToDictionary (JObject.Parse(result)), Keywords = new List<string>() };
        }

        public static List<string> JArrayToDictionary(JArray jArray)
        {
            return jArray
                .Select(token => token.ToString())
                .ToList();
        }

        public static Dictionary<string, List<string>> JObjectToDictionary(JObject jObject)
        {
            var dictionary = new Dictionary<string, List<string>>();

            foreach (var kvp in jObject)
            {
                if (kvp.Value is JArray)
                {
                    var s = JArrayToList((JArray)kvp.Value);
                    dictionary[kvp.Key] = s;
                }
                if (kvp.Value is JValue)
                {
                    var s = kvp.Value?.ToString();
                    dictionary[kvp.Key] = new List<string>() { s };
                }
            }

            return dictionary;
        }

        public static List<string> JArrayToList(JArray jArray)
        {
            var list = new List<string>();

            foreach (var item in jArray)
            {
                list.Add(item.Value<string>());
            }

            return list;
        }
    }
}

