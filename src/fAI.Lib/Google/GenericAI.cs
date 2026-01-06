using DynamicSugar;
using Mistral.SDK.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static fAI.GoogleAICompletions.GoogleAICompletionsResponse;
using static System.Net.Mime.MediaTypeNames;

namespace fAI
{
    public class GenericAI : Logger
    {
        public GenericAI(int timeOut = -1, string ApiKey = null, string openAiOrg = null)
        {
            HttpBase._key = Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY");

            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;

            if (ApiKey != null)
                HttpBase._key = ApiKey;

            if (openAiOrg != null)
                HttpBase. _openAiOrg = openAiOrg;
        }
        public GenericAICompletions _completions = null;
        public GenericAICompletions Completions => _completions ?? (_completions = new GenericAICompletions(ApiKey: HttpBase._key));
    }

    public partial class GenericAICompletions : HttpBase 
    {
        public GenericAICompletions(int timeOut = -1, string ApiKey = null, string openAiOrg = null) : base(timeOut, ApiKey, openAiOrg)
        {
            _key = ApiKey;
        }

        public string Create(string prompt, string systemPrompt, string model)
        {
            if (GoogleAI.GetModels().Contains(model))
            {
                HttpBase._key = Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY");
                var googleAIClient = new GoogleAI();
                var p = googleAIClient.Completions.GetPrompt(prompt, systemPrompt, model);
                var url = googleAIClient.Completions.GetUrl(model, _key);
                var r = googleAIClient.Completions.Create(p, url);
                return r.GetText();
            }
            else if (OpenAI.GetModels().Contains(model))
            {
                HttpBase._key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                var openAIClient = new OpenAI();
                var p = new Prompt_GPT_4
                {
                    Messages = new List<GPTMessage>()
                    {
                        new GPTMessage{ Role =  MessageRole.system, Content = systemPrompt },
                        new GPTMessage{ Role =  MessageRole.user, Content = prompt },
                    },
                    Model = model
                };
                var response = openAIClient.Completions.Create(p);
                if (response.Success)
                {
                    var responseText = response.Text;
                    return responseText;
                }
                else return null;
            }
            return "";
        }

        public string TextImprovement(
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
 - If the following text is part of an email content, add at the end 'Thanks, sincerely Frederic Torres'.
 </rules>
 ===================================
            "
           )
        {
            return Create(text, systemPrompt, model);
        }
    }
}
