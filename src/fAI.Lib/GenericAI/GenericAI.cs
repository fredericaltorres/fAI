using DynamicSugar;
using fAI.Google;
using fAI.Util.Strings;
using Mistral.SDK.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using static DynamicSugar.DS;
using static fAI.GenericAI;
using static fAI.GoogleAICompletions;
using static fAI.GoogleAICompletions.GoogleAICompletionsResponse;
using static fAI.OpenAIImage;
using static System.Net.Mime.MediaTypeNames;

namespace fAI
{
    public class GenericAI : HttpBase
    {
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

        public object CreateAgenticLoop(string userPrompt, string model,
           string systemPrompt = null,
           List<AnthropicTool> tools = null,
           FunctionCallers functionCallers = null)
        {
            if (Anthropic.GetModels().Contains(model))
            {
                return new Anthropic(key: base._key).Completions.CreateAgenticLoop(userPrompt, model, systemPrompt, tools, functionCallers);
            }
            else if (GoogleAI.GetModels().Contains(model))
            {
                return new GoogleAI(apiKey: base._key).Completions.CreateAgenticLoop(userPrompt, model, systemPrompt, tools, functionCallers);
            }
            else throw new Exception($"Model {model} not supported for agentic loop.");
        }

        public (string, GenericAI.Contents) Create(string prompt, string systemPrompt, string model, GenericAI.Contents contents = null)
        {
            var orginalModel = model;
            var sw = Stopwatch.StartNew();
            try
            {
                contents = contents == null ? new GenericAI.Contents() : contents;

                contents.Add(new GenericAI.ContentMessage
                {
                    Role = "user", // A conversation always starts with user message
                    Parts = new List<GenericAI.ContentMessagePart> { new GenericAI.ContentMessagePart { Text = prompt } }
                });

                if (Anthropic.GetModels().Contains(model))
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

                    return (answerContent.Text, contents);
                }
                else if (GoogleAI.GetModels().Contains(model))
                {
                    if (string.IsNullOrEmpty(base._key))
                        base._key = Environment.GetEnvironmentVariable("GOOGLE_GENERATIVE_AI_API_KEY");

                    var googleAIClient = new GoogleAI(apiKey: base._key);

                    // Convert GenericAI.Contents to GoogleAICompletionsBody.Contents
                    var googleContents = contents.GetGoogleContents();
                    var p = googleAIClient.Completions.GetPrompt(prompt, systemPrompt, model, googleContents);
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
            finally
            {
                sw.Stop();
                model = orginalModel; // because of the possible modification of the model variable for Anthropic fast mode, we want to log the original model name.
                OpenAI.Trace(new { model, duration = sw.ElapsedMilliseconds / 1000.0 }, this);
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
- Output ONLY the Markdown content, no preamble or explanation"
           )
        {
            var model = Anthropic.GetModels().FirstOrDefault();
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
            systemPrompt = systemPrompt.Template(new { language }, "[", "]");
            if(summarizeWordCount > 0)
                systemPrompt = systemPrompt.Replace("<rules>", $"<rules>\r\n- Summarize the text in about {summarizeWordCount} words.\r\n");

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

If the phrase contains the words (""list"" or ""research"" or ""find"" or ""determine"") and is asking for information, 
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
            "
           )
        {
            systemPrompt = systemPrompt.Template(new { text }, "[", "]");
            var sw = Stopwatch.StartNew();
            var (json, _) = Create(text, systemPrompt, model);
            sw.Stop();
            var o = DetermineTheTypeOfPhraseResult.FromJson(json);
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
            var (answer, _) = Create(question, systemPrompt, model);
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
            var (answer, _) = Create(prompt, systemPrompt, model);
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

            var (json, _) = Create(text, systemPrompt, model);

            var result = new HttpBase().GetJsonObject(json).ToString();
            return new AIMetaData { MetaData = JObjectToDictionary (JObject.Parse(result)) };
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
