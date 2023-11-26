namespace fAI
{
    public class VoiceDefinitionBase : Local
    {
        public string DisplayName { get; set; }
        public string Gender { get; set; }



        public string VoiceType { get; set; }

        /// <summary>
        /// Id and ShortName are the same
        /// </summary>
        public string Id => this.ShortName;
    }
}

