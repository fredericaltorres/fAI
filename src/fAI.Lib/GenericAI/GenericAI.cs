using DynamicSugar;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static DynamicSugar.DS;
using static fAI.GoogleAICompletions.GoogleAICompletionsResponse;
using static System.Net.Mime.MediaTypeNames;

namespace fAI
{
    public class GenericAI : HttpBase
    {
        public class Contents : List<ContentMessage>
        {

        }

        public class ContentMessagePart
        {
            [JsonProperty("text")]
            public string Text { get; set; }
        }

        public class ContentMessage 
        {
            [JsonProperty("role")]
            public string Role { get; set; }
            public List<ContentMessagePart> Parts { get; set; }
        }

        public static List<string> GetModels()
        {
            var models = new List<string>();
            models.AddRange(Anthropic.GetModels());
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

        public GenericAIImage _images = null;
        public GenericAIImage Images => _images ?? (_images = new GenericAIImage(apiKey: base._key));
    }

    public partial class GenericAICompletions : HttpBase 
    {
        public GenericAICompletions(int timeOut = -1, string ApiKey = null) : base(timeOut, ApiKey)
        {
            _key = ApiKey;
        }

        public (string, GenericAI.Contents)Create(string prompt, string systemPrompt, string model, GenericAI.Contents contents = null)
        {
            contents = contents == null ? new GenericAI.Contents() : contents;

            contents.Add(new GenericAI.ContentMessage
            {
                Role = "user", // A conversation always starts with user message
                Parts = new List<GenericAI.ContentMessagePart>
                    {
                        new GenericAI.ContentMessagePart { Text = prompt }
                    }
            });

            if (Anthropic.GetModels().Contains(model))
            {
                var p = new Anthropic_Prompt_Generic(model)
                {
                    System = systemPrompt,
                    Messages = new List<AnthropicMessage>()
                    {
                        new AnthropicMessage { Role =  MessageRole.user,
                             Content = DS.List<AnthropicContentMessage>(new AnthropicContentText(prompt))
                        }
                    }
                };
                if (string.IsNullOrEmpty(base._key))
                    base._key = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
                var response = new Anthropic(key: base._key).Completions.Create(p);
                return (response.Text, contents);
            }
            else if (GoogleAI.GetModels().Contains(model))
            {
                if(string.IsNullOrEmpty(base._key))
                    base._key = Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY");

                var googleAIClient = new GoogleAI(apiKey: base._key);
                var p = googleAIClient.Completions.GetPrompt(prompt, systemPrompt, model);
                var url = googleAIClient.Completions.GetUrl(model);
                var r = googleAIClient.Completions.Create(p, url, model);

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
                
                return (r.GetText(), contents);
            }
            else if (OpenAI.GetModels().Contains(model))
            {
                if (string.IsNullOrEmpty(base._key))
                    base._key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

                var openAIClient = new OpenAI(apiKey: base._key);
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

                    var responseText = response.Text;
                    return (responseText, contents);
                }
                else return (null, contents);
            }
            return (null, contents);
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
 - If the following question is part of an email content, add at the end 'Thanks, sincerely Frederic Torres'.
 </rules>
 ===================================
            ",
           GenericAI.Contents contents = null
           )
        {
            var sw = Stopwatch.StartNew();
            systemPrompt = systemPrompt.Template(new { language }, "[", "]");
            var (newText, contents2) = Create(text, systemPrompt, model, contents);
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
                if (string.IsNullOrWhiteSpace(text))
                    return 0;

                // Split by whitespace characters and remove empty entries
                string[] words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                return words.Length;
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
            var (response, _) = Create(text, systemPrompt, model);
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
            var (summary, _) = Create(text, systemPrompt, model);
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
            var (translatedText, _) = Create(text, systemPrompt, model);
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
            var (title, _) = Create(text, systemPrompt, model);
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
            var (bulletPointsText, _) = Create(text, systemPrompt, model);
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
            var (jsonAnswer, _) = Create(userPrompt, systemPrompt, model);
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
