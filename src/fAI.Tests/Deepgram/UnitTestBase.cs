using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace fAI.Tests
{
    public class UnitTestBase 
    {
        protected string FlexStrCompare(string s)
        {
            return s.ToLowerInvariant().Replace(",", ".");
        }
        public void AssertWords(string text, List<string> words)
        {
            foreach (var w in words)
                Assert.Contains(w.ToLower(), text.ToLower());
        }

        public void AssertWords(string text, string words)
        {
            AssertWords(text, words.Split(',').ToList());
        }

        public string GetTestFile(string fileName)
        {
            var imageFileName = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", fileName);
            Assert.True(File.Exists(imageFileName));
            return imageFileName;
        }

        public UnitTestBase()
        {
            OpenAI.TraceOn = true;
        }
    }
}