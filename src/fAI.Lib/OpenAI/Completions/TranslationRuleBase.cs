namespace fAI
{
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
}

