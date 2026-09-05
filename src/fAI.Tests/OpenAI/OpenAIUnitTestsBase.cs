using fAI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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

        public static bool AreFloatsEqual(float a, float b, int significantDigits = 5)
        {
            if (a == b) return true;

            double scale = Math.Pow(10, significantDigits);
            return Math.Round(a * scale) == Math.Round(b * scale);
        }

        public static bool AreListOfFloatsEqual(IEnumerable<float> a, IEnumerable<float> b, int significantDigits = 5)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;

            var listA = a.ToList();
            var listB = b.ToList();

            if (listA.Count != listB.Count) return false;

            return listA.Zip(listB, (x, y) => AreFloatsEqual(x, y, significantDigits))
                        .All(equal => equal);
        }

        public OpenAIUnitTestsBase()
        {
            OpenAI.TraceOn = true;
        }

        public void DeleteFile(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }
}
