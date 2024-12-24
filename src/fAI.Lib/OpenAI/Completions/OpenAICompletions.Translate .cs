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

    
        public string Translate(string text, TranslationLanguages sourceLangague, TranslationLanguages targetLanguage, 
            bool applyCustomRule = true,
            List<TranslationOverrideRule> translationOverrideRules = null)
        {
            if (applyCustomRule && NeedToBeTranslated(text, sourceLangague, targetLanguage))
            {
                var prompt = new Prompt_GPT_35_TurboInstruct
                {
                    Text = $"Translate the following {sourceLangague} text to {targetLanguage}: '{text}'",
                };
                var translatedText = Create(prompt).Text.Trim();
                if (translationOverrideRules != null)
                {
                    var translationOverrideRulesForLanguage = translationOverrideRules.Where(ru => ru.MatchLanguages(sourceLangague, targetLanguage));
                    foreach(var rule in translationOverrideRulesForLanguage)
                        if (rule.IsMatch(text, translatedText))
                            translatedText = rule.Replacement;
                }
                return translatedText;
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
            var completionResponse = Create(prompt);
            if (completionResponse.Success)
            {
                if (IsValidJson<Dictionary<string, string>>(completionResponse.Text.Trim()))
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(completionResponse.Text.Trim());
                else 
                    throw new ChatGPTException($"{nameof(Translate)}(), failed  dictionary sourceLangague:{sourceLangague}, json:{json}, targetLanguage:{targetLanguage}, response:{completionResponse}");
            }
            throw new ChatGPTException($"{nameof(Translate)}(), failed to translate dictionary sourceLangague:{sourceLangague}, json:{json}, targetLanguage:{targetLanguage}, response:{completionResponse}");
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

