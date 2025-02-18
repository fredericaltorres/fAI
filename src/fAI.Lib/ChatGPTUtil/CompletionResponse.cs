using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using DynamicSugar;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net.Http.Headers;

namespace fAI
{

    public class CompletionChoiceResponse
    {
        public string text { get; set; }
        public int index { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
        public GPTMessage message { get; set; }
    }

    public class AnthropicCompletionContentResponse
    {
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class CompletionResponse  : BaseHttpResponse
    {
        internal const string NEED_MORE_TOKENS_RETURN_CODE = "length";
        internal const string FULL_SUCCEES_RETURN_CODE = "stop";

        public static List<string> ChatGPTSuccessfullReasons = new List<string> { NEED_MORE_TOKENS_RETURN_CODE, FULL_SUCCEES_RETURN_CODE };

        public GPTPrompt GPTPrompt { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "object")]
        public string @Object { get; set; }

        public int created { get; set; }

        [JsonProperty(PropertyName = "model")]
        public string Model { get; set; }

        // https://platform.openai.com/docs/api-reference/completions/get-completion

        [JsonProperty(PropertyName = "choices")]
        public List<CompletionChoiceResponse> Choices { get; set; }

        [JsonProperty(PropertyName = "content")]
        public List<AnthropicCompletionContentResponse> AnthropicContent { get; set; }

        [JsonProperty(PropertyName = "message")]
        public List<GPTMessage> Message { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public Usage Usage { get; set; }

        public static CompletionResponse FromJson(string json)
        {
            var r = JsonUtils.FromJSON<CompletionResponse>(json);

            if(r.AnthropicContent != null && r.AnthropicContent.Count > 0) // Anthropic and OpenAi have different structure, so I merge Anthropic into OpenAI structure 
            {
                r.Choices = new List<CompletionChoiceResponse>();
                r.AnthropicContent.ForEach(c =>
                {
                    if (c.type == "text")
                        r.Choices.Add(new CompletionChoiceResponse { text = c.text, finish_reason = "stop" });
                });
            }
            
            return r;
        }

        public JArray JsonArray
        {
            get
            {
                if (string.IsNullOrEmpty(this.Text))
                    return null;
                if (this.Text.StartsWith("["))
                {
                    var jsons = ReFormatJsonString(this.Text, "[", "]", justExtract: true, extractAndFormat: false);
                    OpenAI.Trace(jsons[0], this);
                    return JArray.Parse(jsons[0]);
                }
                return null;
            }
        }

        public JObject JsonObject
        {
            get
            {
                if(string.IsNullOrEmpty(this.Text))
                    return null;
                if(this.Text.StartsWith("{"))
                {
                    var jsons = ReFormatJsonString(this.Text, "{", "}", justExtract: true, extractAndFormat: false);
                    OpenAI.Trace(jsons[0], this);
                    return JObject.Parse(jsons[0]);
                }
                return null;
            }
        }

        public class CodeGeneratedInformation
        {
            public string Code { get; set; }
            public string Language { get; set; }
        }

        public CodeGeneratedInformation SourceCode
        {
            get
            {
                if(!this.Text.IsNullOrEmpty())
                {
                    var rx = new Regex(@"```(.*?)```", RegexOptions.Singleline);
                    var m = rx.Match(this.Text);
                    if(m.Success)
                    {
                        var r = m.Groups[1].Value;
                        var lines = r.Split('\n');
                        return new CodeGeneratedInformation {
                            Language = lines[0].Trim(),
                            Code = string.Join(Environment.NewLine, lines.Skip(1))
                        };
                    }
                }
                return null;
            }
        }

        public T Deserialize<T>()
        {
            var json = this.ExtractJsonString(this.Text, justExtract: true)[0];
            return JsonConvert.DeserializeObject<T>(json);
        }

        public string Text {
            get
            {
                if (Choices.Count > 0) 
                {
                    if (!string.IsNullOrEmpty(Choices[0]?.text?.Trim()))
                        return Choices[0].text.Trim();
                    if (Choices[0].message != null)
                        return Choices[0].message.Content.Replace("\n\n", Environment.NewLine);
                }
                return null;
            }
        }

        public static string FormatChatGPTAnswerForTextDisplay(string blogPost)
        {
            var isBulletPointLineRegEx = new System.Text.RegularExpressions.Regex(@"^(\d\. \*\*)|(\d\. )|(- \*\*)|(- `)");

            var isBulletPointLine = new Func<string, bool>((line) => {

                return isBulletPointLineRegEx.IsMatch(line);
            });

            var isDigitBulletPointLineRegEx = new System.Text.RegularExpressions.Regex(@"^(\d\. )");

            var isDigitBulletPointLine = new Func<string, bool>((line) => {

                return isDigitBulletPointLineRegEx.IsMatch(line);
            });
            /*
                 if the line start line this: 1. **Timestamp**: The event occurred ...
                 we do nothing
                 else
                 we need to replace the dot by a new line to have a better display
             */

            ; // ChatGPT return \n instead of \r\n, todo fix it in the API fAI
            blogPost = blogPost.Replace(Environment.NewLine, "`r`n")
                               .Replace("\n", Environment.NewLine)
                               .Replace("`r`n", Environment.NewLine);

            var lines = blogPost.SplitByCRLF().TrimEnd();
            var newLines = new List<string>();
            var x = 0;
            const string DotTag = ". ";
            const string IndentTag = "   ";
            var charMarker = (char)1;
            while (x < lines.Count)
            {
                var currentLine = lines[x];
                var nextLine = x + 1 < lines.Count ? lines[x + 1] : null;

                if (currentLine.Contains(DotTag) && !isBulletPointLine(currentLine))
                {
                    currentLine = currentLine.Replace(DotTag, DotTag + Environment.NewLine);
                    var currentLines = currentLine.SplitByCRLF().TrimEnd();
                    newLines.AddRange(currentLines);
                }
                else if (currentLine.Contains(DotTag) && isBulletPointLine(currentLine))
                {
                    var hasDigitBulletPoint = isDigitBulletPointLine(currentLine);
                    if (hasDigitBulletPoint) // If the line start with 1., we need to ignore the first dot
                    {
                        var xx = currentLine.IndexOf(".");
                        currentLine = currentLine.ReplaceChar(xx, charMarker);
                    }

                    currentLine = currentLine.Replace(DotTag, DotTag + Environment.NewLine);
                    currentLine = currentLine.Replace(charMarker, '.'); // Replace back the first dot, from 1.
                    var currentLines = currentLine.SplitByCRLF().TrimEnd();
                    newLines.AddRange(currentLines.Indent(IndentTag, skipFirstOne: true));
                }
                else
                {
                    newLines.Add(currentLine);
                }
                x++;
            }

            blogPost = string.Join(Environment.NewLine, newLines) + Environment.NewLine;
            return blogPost;
        }

        public string Answer
        {
            get
            {
                var sb = new StringBuilder(1024);

                sb.AppendLine($"Model: {this.GPTPrompt.Model}, Execution: {this.Stopwatch.ElapsedMilliseconds / 1000:0:0}s").AppendLine();
                sb.AppendLine($"Answer:").AppendLine(this.Text);

                return sb.ToString();
            }
        }

        public string BlogPost
        {
            get
            {
                var sb = new StringBuilder(1024);

                sb.AppendLine($"Model: {this.GPTPrompt.Model}, Execution: {this.Stopwatch.ElapsedMilliseconds / 1000:0:0}s").AppendLine();

                var messages = string.Join(Environment.NewLine, this.GPTPrompt.Messages);
                sb.AppendLine($"Messages:").AppendLine(messages);

                sb.AppendLine().AppendLine("".PadLeft(80, '-')).AppendLine();

                sb.AppendLine($"Answer:").AppendLine(this.Text);

                sb.AppendLine().AppendLine("".PadLeft(80, '-')).AppendLine();

                return sb.ToString();
            }
        }

        // https://platform.openai.com/docs/api-reference/completions/object
        public bool Success => Choices != null && Choices.Count > 0 && ChatGPTSuccessfullReasons.Contains(Choices[0].finish_reason);

        public bool NeedMoreToken => Choices.Count > 0 && ChatGPTSuccessfullReasons.Contains(NEED_MORE_TOKENS_RETURN_CODE);

        public string ErrorMessage => this.Exception != null ? this.Exception.Message : "Unknown error";

        public DateTime Created => DateTimeOffset.FromUnixTimeSeconds(created).DateTime;

        public static string RemoveSection(string text)
        {
            return text.Substring(text.IndexOf(" ") + 1);
        }

        public static bool StartsWithANumberSection(string text)
        {
            var rx = new Regex(@"(^\d\. )|(^\d\) )"); // section  from 1..9
            return rx.IsMatch(text);
        }

        public static bool StartsWithALetterSection(string text)
        {
            var rx = new Regex(@"(^\[A-Z]\. )|(^[A-Z]\) )");
            return rx.IsMatch(text);
        }

        public static bool StartsWithWordSection(string text, string word, char sectionChar = ':')
        {
            return text.StartsWith($"{sectionChar}: ");
        }

        public static bool StartsWithWordSection(string text, List<string> words, char sectionChar = ':')
        {
            return words.Any(w => StartsWithWordSection(text, w, sectionChar));
        }

        public List<string> ExtractJsonString(string text, bool justExtract = false, bool extractAndFormat = false)
        {
            var r = new List<string>();
            r = ReFormatJsonString(text, "{", "}", justExtract, extractAndFormat);
            if (r.Count == 0)
                r = ReFormatJsonString(text, "[", "]", justExtract, extractAndFormat);
            return r;
        }


        public object Parse(string json)
        {
            try
            {
                var o = JsonConvert.DeserializeObject(json);
                return o;
            }
            catch
            {
                return null;
            }
        }

        // This function only detect a piece of json in a string and re-format it and does not extract it 
        public List<string> ReFormatJsonString(string text, string jsonStartChar = "{", string jsonEndChar = "}", bool justExtract = false, bool extractAndFormat = false)
        {
            if (text.Contains("`r"))
                text = text.Replace("`r", "");
            if (text.Contains("`n"))
                text = text.Replace("`n", "");

            var r = new List<string>();
            try
            {
                var startSearchIndex = 0;
                var endSearchIndex = text.Length;
                var loop1Counter = 0;
                if (loop1Counter++ == 100) return r;

                var beforeText = string.Empty;
                var afterText = string.Empty;

                var xStart = text.IndexOf(jsonStartChar, startSearchIndex);
                if (xStart != -1)
                {
                    if (xStart > 0) // We found some non json text before the {
                    {
                        beforeText = text.Substring(0, xStart);
                    }

                    var xEnd = text.LastIndexOf(jsonEndChar, endSearchIndex);
                    if (xEnd != -1)
                    {
                        if (xEnd < text.Length) // We found some non json text after the }
                        {
                            afterText = text.Substring(xEnd + 1);
                        }

                        var charCountToGrab = xEnd - xStart + 1;
                        if (charCountToGrab <= 0)
                            return r; // Invalid json ]1,2,3[

                        var json = text.Substring(xStart, charCountToGrab);
                        var o = Parse(json);
                        if (o != null) // We find a valid JSON block
                        {
                            if (justExtract)
                            {
                               r.Add(json);
                            }
                            else
                            {
                                r.Add($"{beforeText}{o}{afterText}");
                            }
                            return r;
                        }
                        else
                        {
                        }
                    }
                }
            }
            catch
            {

            }
            return r;
        }
    }
}


