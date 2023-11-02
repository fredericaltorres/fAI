using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace fAI
{
    public class ChatGPTTranslationResponseChoice
    {
        public string text { get; set; }
        public int index { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
    }

    public class ChatGPTResponse
    {
        private const string NEED_MORE_TOKENS_RETURN_CODE = "length";
        private const string FULL_SUCCEES_RETURN_CODE = "stop";

        public static List<string> ChatGPTSuccessfullReasons = new List<string> { NEED_MORE_TOKENS_RETURN_CODE, FULL_SUCCEES_RETURN_CODE };

        public string id { get; set; }
        public string @object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public List<ChatGPTTranslationResponseChoice> choices { get; set; }
        public Usage usage { get; set; }

        public static ChatGPTResponse FromJson(string json) => JsonConvert.DeserializeObject<ChatGPTResponse>(json);
        public string Text => choices.Count > 0 ? choices[0].text.Trim() : null;

        // https://platform.openai.com/docs/api-reference/completions/object
        public bool Success => choices.Count > 0 && ChatGPTSuccessfullReasons.Contains(choices[0].finish_reason);

        public bool NeedMoreToken => choices.Count > 0 && ChatGPTSuccessfullReasons.Contains(NEED_MORE_TOKENS_RETURN_CODE);

        public string ErrorMessage => "Error"; // TODO: Implement

        public DateTime Created => DateTimeOffset.FromUnixTimeSeconds(created).DateTime;
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }
}


