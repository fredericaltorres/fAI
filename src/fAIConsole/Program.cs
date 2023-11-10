using fAI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fAIConsole
{
    internal class Program
    {
        const string EnglishTest01 = @"I am he as you are he as you are me
And we are all together.
See how they run like pigs from a gun
See how they fly.
I'm crying";

        static void Main(string[] args)
        {
            var mcs = new MicrosoftCognitiveServices();
            var voiceId = "en-US-DavisNeural";
            var mp3FileName = Path.Combine(Path.GetTempPath(), "mp3.mp3");
            mcs.ExecuteTTS(EnglishTest01, voiceId, mp3FileName);
        }
    }
}
