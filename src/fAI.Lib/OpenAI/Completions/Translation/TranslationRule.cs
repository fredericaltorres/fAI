using System.Text.RegularExpressions;

namespace fAI
{
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
}

