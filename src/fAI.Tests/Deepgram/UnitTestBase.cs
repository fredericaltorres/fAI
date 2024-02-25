using System.Collections.Generic;
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

    }
}