using DynamicSugar;
using NAudio.SoundFont;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace fAI.OpenAIModel.ImageResponseGpt
{
    //public class OpenAIGpt55ImageResponse

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);


    public class OpenAIGpt55ImageResponse
    {
        public string id { get; set; }
        public string @object { get; set; }
        public int created_at { get; set; }
        public string status { get; set; }
        public bool background { get; set; }
        public Billing billing { get; set; }
        public int completed_at { get; set; }
        public object error { get; set; }
        public double frequency_penalty { get; set; }
        public object incomplete_details { get; set; }
        public object instructions { get; set; }
        public object max_output_tokens { get; set; }
        public object max_tool_calls { get; set; }
        public string model { get; set; }
        public object moderation { get; set; }
        public List<Output> output { get; set; }
        public bool parallel_tool_calls { get; set; }
        public double presence_penalty { get; set; }
        public object previous_response_id { get; set; }
        public object prompt_cache_key { get; set; }
        public string prompt_cache_retention { get; set; }
        public Reasoning reasoning { get; set; }
        public object safety_identifier { get; set; }
        public string service_tier { get; set; }
        public bool store { get; set; }
        public double temperature { get; set; }
        public Text text { get; set; }
        public string tool_choice { get; set; }
        public List<Tool> tools { get; set; }
        public int top_logprobs { get; set; }
        public double top_p { get; set; }
        public string truncation { get; set; }
        public Usage usage { get; set; }
        public object user { get; set; }
        public Metadata metadata { get; set; }

        public static OpenAIGpt55ImageResponse FromJson(string json) => JsonConvert.DeserializeObject<OpenAIGpt55ImageResponse>(json);

        public List<string> GetLocalImages()
        {
            var base64s = GetImagesBase64();
            var images = new List<string>();
            foreach (var base64 in base64s)
            {
                var bytes = Convert.FromBase64String(base64);
                var tfh = new TestFileHelper();
                var tempFile = tfh.GetTempFileName(".png");
                System.IO.File.WriteAllBytes(tempFile, bytes);
                images.Add(tempFile);
            }
            return images;
        }

        public List<string> GetImagesBase64()
        {
            if (this.status == "completed")
            {
                var images = this.output.Where(i => i.type == "image_generation_call").Select(i => i.result).ToList();
                return images;
            }
            else
            {
                throw new Exception($"Response status is {this.status}, cannot get images.");
            }
        }
    }

    public class Billing
    {
        public string payer { get; set; }
    }

    public class Content
    {
        public string type { get; set; }
        public List<object> annotations { get; set; }
        public List<object> logprobs { get; set; }
        public string text { get; set; }
    }

    public class Format
    {
        public string type { get; set; }
    }

    public class InputTokensDetails
    {
        public int cached_tokens { get; set; }
    }

    public class Metadata
    {
    }

    public class Output
    {
        public string id { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string action { get; set; }
        public string background { get; set; }
        public string output_format { get; set; }
        public string quality { get; set; }
        public string result { get; set; }
        public string revised_prompt { get; set; }
        public string size { get; set; }
        public List<Content> content { get; set; }
        public string phase { get; set; }
        public string role { get; set; }
    }

    public class OutputTokensDetails
    {
        public int reasoning_tokens { get; set; }
    }

    public class Reasoning
    {
        public string effort { get; set; }
        public object summary { get; set; }
    }


    public class Text
    {
        public Format format { get; set; }
        public string verbosity { get; set; }
    }

    public class Tool
    {
        public string type { get; set; }
        public string background { get; set; }
        public string model { get; set; }
        public string moderation { get; set; }
        public int n { get; set; }
        public int output_compression { get; set; }
        public string output_format { get; set; }
        public string quality { get; set; }
        public string size { get; set; }
    }

    public class Usage
    {
        public int input_tokens { get; set; }
        public InputTokensDetails input_tokens_details { get; set; }
        public int output_tokens { get; set; }
        public OutputTokensDetails output_tokens_details { get; set; }
        public int total_tokens { get; set; }
    }



}
