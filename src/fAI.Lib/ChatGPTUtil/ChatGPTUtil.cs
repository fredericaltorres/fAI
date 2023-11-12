using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DynamicSugar;

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

    public class CompletionResponse
    {
        private const string NEED_MORE_TOKENS_RETURN_CODE = "length";
        private const string FULL_SUCCEES_RETURN_CODE = "stop";

        public static List<string> ChatGPTSuccessfullReasons = new List<string> { NEED_MORE_TOKENS_RETURN_CODE, FULL_SUCCEES_RETURN_CODE };

        public GPTPrompt GPTPrompt { get; set; }
        public System.Diagnostics.Stopwatch Sw { get; set; }

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

        [JsonProperty(PropertyName = "message")]
        public List<GPTMessage> Message { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public Usage Usage { get; set; }

        public static CompletionResponse FromJson(string json) => JsonConvert.DeserializeObject<CompletionResponse>(json);
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



        public string BlogPost
        {
            get
            {
                var sb = new StringBuilder(1024);

                sb.AppendLine($"Model: {this.GPTPrompt.Model}, Execution: {this.Sw.ElapsedMilliseconds / 1000:0:0}s").AppendLine();

                var messages = string.Join(Environment.NewLine, this.GPTPrompt.Messages);
                sb.AppendLine($"Messages:").AppendLine(messages);

                sb.AppendLine().AppendLine("".PadLeft(80, '-')).AppendLine();

                sb.AppendLine($"Answer:").AppendLine(this.Text);

                sb.AppendLine().AppendLine("".PadLeft(80, '-')).AppendLine();

                return sb.ToString();
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


