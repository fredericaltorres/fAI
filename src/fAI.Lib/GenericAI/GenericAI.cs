using DynamicSugar;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static fAI.GoogleAICompletions.GoogleAICompletionsResponse;
using static System.Net.Mime.MediaTypeNames;

namespace fAI
{
    public class GenericAI : HttpBase
    {
        public static List<string> GetModels()
        {
            var models = new List<string>();
            models.AddRange(GoogleAI.GetModels());
            models.AddRange(OpenAI.GetModels());
            return models;
        }

        public GenericAI(int timeOut = -1, string ApiKey = null, string openAiOrg = null)
        {
            HttpBase._timeout = 60 * 4;

            if (timeOut > 0)
                HttpBase._timeout = timeOut;

            if (ApiKey != null)
                base._key = ApiKey;
        }
        public GenericAICompletions _completions = null;
        public GenericAICompletions Completions => _completions ?? (_completions = new GenericAICompletions(ApiKey: base._key));
    }

    public partial class GenericAICompletions : HttpBase 
    {
        public GenericAICompletions(int timeOut = -1, string ApiKey = null) : base(timeOut, ApiKey)
        {
            _key = ApiKey;
        }

        public string Create(string prompt, string systemPrompt, string model)
        {
            if (GoogleAI.GetModels().Contains(model))
            {
                if(string.IsNullOrEmpty(base._key))
                    base._key = Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY");

                var googleAIClient = new GoogleAI(ApiKey: base._key);
                var p = googleAIClient.Completions.GetPrompt(prompt, systemPrompt, model);
                var url = googleAIClient.Completions.GetUrl(model);
                var r = googleAIClient.Completions.Create(p, url, model);
                return r.GetText();
            }
            else if (OpenAI.GetModels().Contains(model))
            {
                if (string.IsNullOrEmpty(base._key))
                    base._key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

                var openAIClient = new OpenAI(openAiKey: base._key);
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
 - If the following question is part of an email content, add at the end 'Thanks, sincerely Frederic Torres'.
 </rules>
 ===================================
            "
           )
        {
            systemPrompt = systemPrompt.Template(new { language }, "[", "]");
            return Create(text, systemPrompt, model);
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
                if (string.IsNullOrWhiteSpace(text))
                    return 0;

                // Split by whitespace characters and remove empty entries
                string[] words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                return words.Length;
            }
        }

        public SummarizationResult Summarize(
           string text,
           string language,
           string model,
           string systemPrompt = @"
Summarize the following [language] question.
Use the following rules to guide your summarization:
<rules>
- Do NOT use MARKDOWN formatting.
- Insert a new line between paragraphs.
- Maintain the context of the question without altering its meaning.
- Keep the bulletPointsText concise and to the point.
- Use formal language suitable for business communication.
- Ensure that all key information is included in the bulletPointsText.
 </rules>
 ===================================
            "
           )
        {
            systemPrompt = systemPrompt.Template(new { language }, "[", "]");
            var sw  = Stopwatch.StartNew();
            var summary = Create(text, systemPrompt, model);
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
            var translatedText = Create(text, systemPrompt, model);
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
- Use formal language suitable for business communication.
 </rules>
 ===================================
            "
           )
        {
            systemPrompt = systemPrompt.Template(new { language }, "[", "]");
            var sw = Stopwatch.StartNew();
            var title = Create(text, systemPrompt, model);
            sw.Stop();
            return new GenerateTitleResult
            {
                Title = title,
                Duration = sw.ElapsedMilliseconds / 1000.0
            };
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
            var bulletPointsText = Create(text, systemPrompt, model);
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
            var jsonAnswer = Create(userPrompt, systemPrompt, model);
            var answer = base.GetJsonObject(jsonAnswer)["answer"].ToString();
            sw.Stop();
            return new AnswerQuestionBasedOnTextResult
            {
                Text = answer,
                Duration = sw.ElapsedMilliseconds / 1000.0,
                Model = model
            };
        }
    }
}
