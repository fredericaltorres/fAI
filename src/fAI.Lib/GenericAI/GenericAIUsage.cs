using fAI.AnthropicLib;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;

namespace fAI
{

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
}

