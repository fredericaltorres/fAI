using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace fAI.Tests
{
    public class LedSequence
    {
        [JsonPropertyName("sequences")]
        public List<Sequence> Sequences { get; set; }
    }
    
    public class Sequence
    {
        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("actions")]
        public List<Action> Actions { get; set; }
    }

    public class Action
    {
        [JsonPropertyName("command")]
        public string Command { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}

