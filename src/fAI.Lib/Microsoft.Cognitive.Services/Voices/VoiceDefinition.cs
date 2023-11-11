using System.Collections.Generic;

namespace fAI
{
    public class VoiceDefinition : VoiceDefinitionBase
    {
        public string Name { get; set; }
        public string SampleRateHertz { get; set; }
        public string Status { get; set; }
        public string WordsPerMinute { get; set; }
        public List<string> StyleList { get; set; }
        public List<string> SecondaryLocaleList { get; set; }
        public List<string> RolePlayList { get; set; }

        public string GetVoiceDefinitionDisplayName()
        {
            return $"{this.DisplayName}, {this.Gender}, {this.LocaleName}, {this.ShortName}";
        }

        public override string ToString()
        {
            return GetVoiceDefinitionDisplayName();
        }
    }
}

