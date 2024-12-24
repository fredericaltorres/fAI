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
            ""1"": ""There are people who have a significant number of followers in every business domain."",
            ""2"": ""Education"",
            ""3"": ""Classroom 01"",
            ""4"": ""Classroom 02"",
            ""5"": ""Business Charts"",
            ""6"": ""Is a great way to visualize information about users""
        }";

        public OpenAIUnitTestsBase()
        {
            OpenAI.TraceOn = true;
        }
    }
}
