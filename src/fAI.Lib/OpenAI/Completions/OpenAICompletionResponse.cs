using System;
using System.Collections.Generic;
using System.Text;

namespace fAI.OpenAI_Completions_Response
{
   

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Choice
    {
        public int index { get; set; }
        public Message message { get; set; }
        public string finish_reason { get; set; }
    }

    public class CompletionTokensDetails
    {
        public int reasoning_tokens { get; set; }
        public int audio_tokens { get; set; }
        public int accepted_prediction_tokens { get; set; }
        public int rejected_prediction_tokens { get; set; }
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
        public object refusal { get; set; }
        public List<object> annotations { get; set; }
    }

    public class PromptTokensDetails
    {
        public int cached_tokens { get; set; }
        public int audio_tokens { get; set; }
    }

    // Only used to get the usage details, so we can calculate the cost of the request. 
    public class OpenAICompletionResponse
    {
        //public string id { get; set; }
        //public string @object { get; set; }
        //public int created { get; set; }
        //public string model { get; set; }
        //public List<Choice> choices { get; set; }
        public Usage usage { get; set; }
        //public string service_tier { get; set; }
        //public object system_fingerprint { get; set; }


        public static OpenAICompletionResponse FromJson(string text)
        {
            return JsonUtils.FromJSON<OpenAICompletionResponse>(text);
        }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
        public PromptTokensDetails prompt_tokens_details { get; set; }
        public CompletionTokensDetails completion_tokens_details { get; set; }
    }


}
