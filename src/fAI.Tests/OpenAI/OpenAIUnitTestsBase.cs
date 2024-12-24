using System.IO;
using System.Text.RegularExpressions;
using fAI;
using System.Runtime.InteropServices;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace fAI.Tests
{
    public class OpenAIUnitTestsBase : UnitTestBase
    {
        public const string ReferenceEnglishSentence = "Hello world.";

        public const string ReferenceEnglishJsonDictionary = @"{
            ""(50,51)"": ""There are people who have a significant number of followers in every business domain. There are people who have a significant number of followers in every business domain."",
            ""(50,52)"": ""Education "",
            ""(53,54)"": ""Classroom 01"",
            ""(53,55)"": ""Classroom 02"",
            ""(56,57)"": ""Business Charts"",
            ""(56,58)"": ""Is a great way to visualize information about users""
        }";

        public OpenAIUnitTestsBase()
        {
            OpenAI.TraceOn = true;
        }
    }
}
