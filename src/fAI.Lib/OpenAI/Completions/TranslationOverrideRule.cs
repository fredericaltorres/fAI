using System.Text.RegularExpressions;

namespace fAI
{
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
}

