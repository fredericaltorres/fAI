using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace fAI
{
    public partial class OpenAICompletions  : HttpBase
    {
        public OpenAICompletions(int timeOut = -1, string openAiKey = null, string openAiOrg = null) : base(timeOut,  openAiKey, openAiOrg)
        {
        }

        const string __url = "https://api.openai.com/v1/models";

        public Models GetModels()
        {
            var response = InitWebClient().GET(__url);
            if(response.Success)
            {
                return Models.FromJSON(response.Text);
            }
            else throw new ChatGPTException($"{nameof(GetModels)}() failed - {response.Exception.Message}", response.Exception);
        }

        // https://platform.openai.com/docs/guides/gpt
        public CompletionResponse Create(GPTPrompt p)
        {
            OpenAI.Trace(new { p.Url }, this);
            OpenAI.Trace(new { Prompt = p }, this);
            OpenAI.Trace(new { Body = p.GetPostBody() }, this);

            var sw = Stopwatch.StartNew();
            var response = InitWebClient().POST(p.Url, p.GetPostBody());
            sw.Stop();
            if (response.Success)
            {
                response.SetText(response.Buffer, response.ContenType);
                OpenAI.Trace(new { response.Text }, this);

                var r = CompletionResponse.FromJson(response.Text);
                r.GPTPrompt = p;
                r.Stopwatch = sw;
                return r;
            }
            else
            {
                return new CompletionResponse { Exception = new ChatGPTException($"{response.Exception.Message}", response.Exception) };
            }
        }

        public string Summarize(string text, TranslationLanguages sourceLangague) 
        {
            var prompt = new Prompt_GPT_35_TurboInstruct 
            {
                Text = text,
                PrePrompt = "Summarize the following text: \n===\n",
                PostPrompt = "\n===\nSummary:\n",
            };

            return Create(prompt).Text?.Trim();
        }

        public enum GPT_YesNoResponse
        {
            Yes,
            No,
            Unknown
        }

        public GPT_YesNoResponse IsThis(string systemContent, string yesNoQuestion, string dataText, string forceAnswerToYesNo = ", answer only with yes or no?")
        {
            var prompt = new Prompt_GPT_4
            {
                Messages = new List<GPTMessage>()
                {
                    new GPTMessage{ Role =  MessageRole.system, Content = systemContent },
                    new GPTMessage{ Role =  MessageRole.user, Content = $"{yesNoQuestion}{forceAnswerToYesNo}{Environment.NewLine}{dataText}" }
                },
                Url = "https://api.openai.com/v1/chat/completions"
            };
            var response = this.Create(prompt);
            if (response.Success)
            {
                if(response.Text.ToLowerInvariant().Contains(GPT_YesNoResponse.Yes.ToString().ToLower()))
                    return GPT_YesNoResponse.Yes;
                if (response.Text.ToLowerInvariant().Contains(GPT_YesNoResponse.No.ToString().ToLower()))
                    return GPT_YesNoResponse.No;
                return GPT_YesNoResponse.Unknown;
            }
               
            else 
                throw new ChatGPTException($"{nameof(IsThis)}() failed - {response.ErrorMessage}");
        }


        public class TranslationRuleBase
        {
            public TranslationLanguages sourceLangague { get; set; }
            public TranslationLanguages targetLanguage { get; set; }
            public string Name { get; set; }

            public bool MatchLanguages(TranslationLanguages sourceLangague, TranslationLanguages targetLanguage)
            {
                return this.sourceLangague == sourceLangague && this.targetLanguage == targetLanguage;
            }

        }
        public class TranslationOverrideRule : TranslationRuleBase
        {
            public Regex InputTextRegex { get; set; }
            public Regex OutputTextRegex { get; set; }
            public string Replacement { get; set; }

            public bool IsMatch(string inputText, string outputText)
            {
                return InputTextRegex.IsMatch(inputText) && OutputTextRegex.IsMatch(outputText);
            }

            public string Replace(string inputText, string outputText)
            {
                if(IsMatch(inputText, outputText))
                    return this.Replacement;
                return outputText;
            }
        }

        public class TranslationRule  : TranslationRuleBase
        {
            public Regex regex { get; set; } 
            public bool TranslateIfMatch { get; set; }
            
            public bool IsMatch(string text)
            {
                return regex.IsMatch(text);
            }

            public override string ToString()
            {
                return $"{Name}, {sourceLangague} to {targetLanguage}, {regex}, {TranslateIfMatch}";
            }
        }

        public bool NeedToBeTranslated(string text, TranslationLanguages sourceLangague, TranslationLanguages targetLanguage)
        {
            var translationRules = new List<TranslationRule>();
            translationRules.AddRange(LoadEnglishToFrenchRules());

            var applicableRules = translationRules.Where(r => r.MatchLanguages(sourceLangague, targetLanguage));
            foreach (var rule in applicableRules)
                if (rule.IsMatch(text))
                    return rule.TranslateIfMatch;

            return true;
        }

        private static List<TranslationRule> LoadEnglishToFrenchRules()
        {
            var numberRegEx = @"^-?\d+(\.\d+)?$";
            var percentRegEx = @"^-?\d+(\.\d+)?%$";
            var NumberWithKRegEx = @"^-?\d+(\.\d+)?K$";
            var DollardAmountWithKRegEx = @"^\$-?\d+(\.\d+)?K$"; // $200K
            var DollardAmountWithMRegEx = @"^\$-?\d+(\.\d+)?M$"; // $1M
            var numberWithThousandSeparator = @"^\d{1,3}(,\d{3})*(\.\d+)?$";

            return new List<TranslationRule>()
            {
                new TranslationRule { Name = "Percentage with float 100% or 0.22%", sourceLangague = TranslationLanguages.English, targetLanguage = TranslationLanguages.French,
                                      regex = new Regex(percentRegEx, RegexOptions.IgnoreCase), TranslateIfMatch = false },
                new TranslationRule { Name = "Number with k like 200K", sourceLangague = TranslationLanguages.English, targetLanguage = TranslationLanguages.French,
                                      regex = new Regex(NumberWithKRegEx, RegexOptions.IgnoreCase), TranslateIfMatch = false },
                new TranslationRule { Name = "Just a number", sourceLangague = TranslationLanguages.English, targetLanguage = TranslationLanguages.French,
                                      regex = new Regex(numberRegEx, RegexOptions.IgnoreCase), TranslateIfMatch = false },
                new TranslationRule { Name = "$ amount with a m, $1M do not translate because too long", sourceLangague = TranslationLanguages.English, targetLanguage = TranslationLanguages.French,
                                      regex = new Regex(DollardAmountWithMRegEx, RegexOptions.IgnoreCase), TranslateIfMatch = false },

                new TranslationRule { Name = "$ amount with a k, Chat GPT do a good job translating", sourceLangague = TranslationLanguages.English, targetLanguage = TranslationLanguages.French,
                                      regex = new Regex(DollardAmountWithKRegEx, RegexOptions.IgnoreCase), TranslateIfMatch = true },
                new TranslationRule { Name = "Number with , as a thousand separator, Chat GPT do a good job translating", sourceLangague = TranslationLanguages.English, targetLanguage = TranslationLanguages.French,
                                      regex = new Regex(numberWithThousandSeparator, RegexOptions.IgnoreCase), TranslateIfMatch = true },
            };
        }

        private bool IsValidJson<T>(string json)
        {
            try 
            {                 
                var o = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsNumeric(string value)
        {
            return value.All(char.IsNumber);
        }

        private bool IsNumeric(List<string> strings)
        {
            return strings.All(x => IsNumeric(x));
        }

        public string Translate(string text, TranslationLanguages sourceLangague, TranslationLanguages targetLanguage, bool applyCustomRule = true, List<TranslationRule> translationRules = null)
        {
            if (applyCustomRule && NeedToBeTranslated(text, sourceLangague, targetLanguage))
            {
                var prompt = new Prompt_GPT_35_TurboInstruct
                {
                    Text = $"Translate the following {sourceLangague} text to {targetLanguage}: '{text}'",
                };
                return Create(prompt).Text.Trim();
            }
            else return text;
        }

        public Dictionary<string, string> Translate(Dictionary<string, string> inputDictionary, TranslationLanguages sourceLangague, TranslationLanguages targetLanguage,
             bool applyCustomRule = true, // todo" implement
             List<TranslationRule> translationRules = null // todo" implement
            )
        {
            var strings = inputDictionary.Values.ToList();
            if (IsNumeric(strings)) // GPT does not like things that cannot be translated and return an invalid JSON
                return inputDictionary;

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(inputDictionary, formatting: Formatting.Indented);
            var prompt = new Prompt_GPT_35_TurboInstruct
            {
                Text = $"Translate from {sourceLangague} to {targetLanguage} the following JSON blob:\r\n{json}",
            };
            var responseJson = Create(prompt).Text.Trim();
            if (IsValidJson<Dictionary<string, string>>(responseJson))
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);
            else
                throw new ChatGPTException($"{nameof(Translate)}(), failed to translate dictionary sourceLangague:{sourceLangague}, json:{json}, targetLanguage:{targetLanguage}, response:{responseJson}");
        }


        public List<string> Translate(List<string> strings, TranslationLanguages sourceLangague, TranslationLanguages targetLanguage,
            bool applyCustomRule = true, // todo" implement
             List<TranslationRule> translationRules = null // todo" implement
            )
        {
            if (IsNumeric(strings)) // GPT does not like things that cannot be translated and return an invalid JSON
                return strings;

            var intKey = 0;
            var d = strings.ToDictionary(x => (intKey++).ToString(), x => x);
            var translatedStrings = Translate(d, sourceLangague, targetLanguage).Values.ToList();
            var finalStrings = new List<string>();

            // Ignore the translation of text we do not want to translate
            for(var x = 0; x<strings.Count; x++)
            {
                var sourceText = strings[x];
                if (applyCustomRule && NeedToBeTranslated(sourceText, sourceLangague, targetLanguage))
                    finalStrings.Add(translatedStrings[x]);
                else 
                    finalStrings.Add(sourceText);
            }
            return finalStrings;
        }
    }
}

