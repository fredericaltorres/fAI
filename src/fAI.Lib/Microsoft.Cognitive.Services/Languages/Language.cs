using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fAI
{
    public class Language
    {
        public string Code { get; set; }
        public string Accent { get; set; }
        public string Name { get; set; }
        public string Native { get; set; }

        public override string ToString()
        {
            return $"{Code} - {Name}";
        }
    }
}
