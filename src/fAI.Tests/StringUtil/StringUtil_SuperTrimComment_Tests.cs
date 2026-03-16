using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using fAI;
using Xunit;
using static fAI.OpenAICompletions;
using System.Runtime.InteropServices;
using fAI.Util.Strings;

namespace fAI.Tests
{
    [Collection("Sequential")]
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class StringUtil_SuperTrimComment_Tests : UnitTestBase
    {
        [Fact]
        public void NullInput_ReturnsNull()
        {
            string result = StringUtil.SuperTrimComment(null);
            Assert.Null(result);
        }

        [Fact]
        public void EmptyString_ReturnsEmpty()
        {
            string result = StringUtil.SuperTrimComment(string.Empty);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void WhitespaceOnly_ReturnsWhitespace()
        {
            string result = StringUtil.SuperTrimComment("   ");
            Assert.Equal("   ", result);
        }

        [Fact]
        public void NoParentheses_ReturnsOriginalString()
        {
            string result = StringUtil.SuperTrimComment("Hello World");
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void NumbersAndSymbols_NoParens_ReturnsOriginal()
        {
            string result = StringUtil.SuperTrimComment("abc 123 !@#$%");
            Assert.Equal("abc 123 !@#$%", result);
        }

        [Fact]
        public void SingleGroup_InMiddle_RemovesGroup()
        {
            string result = StringUtil.SuperTrimComment("Hello (this is a comment) World");
            Assert.Equal("Hello  World", result);
        }

        [Fact]
        public void SingleGroup_AtStart_RemovesGroup()
        {
            string result = StringUtil.SuperTrimComment("(comment) Hello");
            Assert.Equal(" Hello", result);
        }

        [Fact]
        public void SingleGroup_AtEnd_RemovesGroup()
        {
            string result = StringUtil.SuperTrimComment("Hello (comment)");
            Assert.Equal("Hello ", result);
        }

        [Fact]
        public void SingleGroup_EntireString_ReturnsEmpty()
        {
            string result = StringUtil.SuperTrimComment("(entire string is a comment)");
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void EmptyParentheses_RemovesParentheses()
        {
            string result = StringUtil.SuperTrimComment("Hello () World");
            Assert.Equal("Hello  World", result);
        }

        [Fact]
        public void MultipleGroups_RemovesAll()
        {
            string result = StringUtil.SuperTrimComment("Multiple (first) and (second) groups");
            Assert.Equal("Multiple  and  groups", result);
        }

        [Fact]
        public void MultipleAdjacentGroups_RemovesAll()
        {
            string result = StringUtil.SuperTrimComment("Start (one)(two)(three) End");
            Assert.Equal("Start  End", result);
        }

        [Fact]
        public void NestedParentheses_RemovesAll()
        {
            string result = StringUtil.SuperTrimComment("Remove (nested (inner) content) here");
            Assert.Equal("Remove  here", result);
        }

        [Fact]
        public void DeeplyNestedParentheses_RemovesAll()
        {
            string result = StringUtil.SuperTrimComment("Start (a (b (c (d)))) End");
            Assert.Equal("Start  End", result);
        }

        [Fact]
        public void MultipleNestedGroups_RemovesAll()
        {
            string result = StringUtil.SuperTrimComment("(first (nested)) middle (second (nested)) end");
            Assert.Equal(" middle  end", result);
        }

        [Fact]
        public void UnmatchedOpenParen_LeavesUnchanged()
        {
            string result = StringUtil.SuperTrimComment("Hello (World");
            Assert.Equal("Hello (World", result);
        }

        [Fact]
        public void UnmatchedCloseParen_LeavesUnchanged()
        {
            string result = StringUtil.SuperTrimComment("Hello World)");
            Assert.Equal("Hello World)", result);
        }

        [Fact]
        public void MixedMatchedAndUnmatched_RemovesOnlyMatched()
        {
            string result = StringUtil.SuperTrimComment("Hello (matched) World (unmatched");
            Assert.Equal("Hello  World (unmatched", result);
        }

        [Fact]
        public void SpecialCharsInParens_RemovesGroup()
        {
            string result = StringUtil.SuperTrimComment("Price (€100 + $50 / 2!) now");
            Assert.Equal("Price  now", result);
        }

        [Fact]
        public void NewlineInsideParens_RemovesGroup()
        {
            string result = StringUtil.SuperTrimComment("Hello (line1\nline2) World");
            Assert.Equal("Hello  World", result);
        }

        [Fact]
        public void NumbersInsideParens_RemovesGroup()
        {
            string result = StringUtil.SuperTrimComment("Version (1.2.3) released");
            Assert.Equal("Version  released", result);
        }

        [Fact]
        public void AfterRemoval_ExtraSpacesCanBeNormalized()
        {
            // Demonstrates the optional whitespace-cleanup step
            string raw = StringUtil.SuperTrimComment("Hello (comment) World");
            string normalized = System.Text.RegularExpressions.Regex
                .Replace(raw, @"\s{2,}", " ").Trim();

            Assert.Equal("Hello World", normalized);
        }
    }
}