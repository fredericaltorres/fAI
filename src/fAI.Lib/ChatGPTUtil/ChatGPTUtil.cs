using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace fAI
{
    public class ChatGPTTranslationResponseChoice
    {
        public string text { get; set; }
        public int index { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
        public GPTMessage message { get; set; }
    }

    public class ChatGPTResponse
    {
        private const string NEED_MORE_TOKENS_RETURN_CODE = "length";
        private const string FULL_SUCCEES_RETURN_CODE = "stop";

        public static List<string> ChatGPTSuccessfullReasons = new List<string> { NEED_MORE_TOKENS_RETURN_CODE, FULL_SUCCEES_RETURN_CODE };

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "object")]
        public string @Object { get; set; }

        public int created { get; set; }

        [JsonProperty(PropertyName = "model")]
        public string Model { get; set; }

        [JsonProperty(PropertyName = "choices")]
        public List<ChatGPTTranslationResponseChoice> Choices { get; set; }

        [JsonProperty(PropertyName = "message")]
        public List<GPTMessage> Message { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public Usage Usage { get; set; }

        public static ChatGPTResponse FromJson(string json) => JsonConvert.DeserializeObject<ChatGPTResponse>(json);
        public string Text {
            get
            {
                if (Choices.Count > 0) 
                {
                    if (!string.IsNullOrEmpty(Choices[0]?.text?.Trim()))
                        return Choices[0].text.Trim();
                    if (Choices[0].message != null)
                        return Choices[0].message.Content;
                }
                return null;
            }
        }

        // https://platform.openai.com/docs/api-reference/completions/object
        public bool Success => Choices.Count > 0 && ChatGPTSuccessfullReasons.Contains(Choices[0].finish_reason);

        public bool NeedMoreToken => Choices.Count > 0 && ChatGPTSuccessfullReasons.Contains(NEED_MORE_TOKENS_RETURN_CODE);

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


