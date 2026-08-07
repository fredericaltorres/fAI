using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace fAI.Util.Strings
{
    public static class StringUtil
    {
        public static List<string> GetCommonElements(List<string> l1, List<string> l2)
        {
            return l1.Intersect(l2).ToList();
        }

        public static bool ContainsAllKeywords(List<string> keywords, string text)
        {
            if (keywords == null || text == null)
                return false;

            string lowerText = text.ToLowerInvariant();
            return keywords.All(keyword => lowerText.Contains(keyword.ToLowerInvariant()));
        }

        public const string REQUIRED_KEYWORD_PREFIX = "~";

        public static List<string> ExtractBackTilda(string input)
        {
            var matches = Regex.Matches(input, $@"{REQUIRED_KEYWORD_PREFIX}(\w+)");
            return matches
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .Select(s => s.Trim())
                .ToList();
        }

        public static (string Keyword, string Cleaned) ExtractAndRemoveQueryRequiredKeywords(string input)
        {
            if (string.IsNullOrEmpty(input))
                return (null, input);

            string trimmed = input.TrimEnd();

            // Must end with "))"
            if (!trimmed.EndsWith("))", StringComparison.Ordinal))
                return (null, input);

            // Find the matching "((" for the trailing "))"
            int openIndex = trimmed.LastIndexOf("((", StringComparison.Ordinal);
            if (openIndex < 0)
                throw new ArgumentException("Input string must contain a matching '((' for the trailing '))'.");

            int closeIndex = trimmed.Length - 2; // position of "))"
            if (closeIndex <= openIndex + 1)
                return (null, input); // malformed / nothing in between

            // Extract keyword between "((" and "))"
            string keyword = trimmed.Substring(openIndex + 2, closeIndex - (openIndex + 2)).Trim();

            // Remove the "((...))" segment (including the markers) from the original string
            string before = trimmed.Substring(0, openIndex);
            string after = trimmed.Substring(closeIndex + 2);
            string cleaned = (before + after).Trim();

            return (keyword.Trim(), cleaned.Trim());
        }

        public static string CapitalizeWords(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string[] words = input.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 1)
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }

            return string.Join(" ", words);
        }

        public static string SuperTrimComment(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove all content within parentheses (including nested ones) and the parentheses themselves
            int safetyLimit = 100; // prevent infinite loops for malformed input
            while (safetyLimit-- > 0)
            {
                string result = Regex.Replace(input, @"\([^()]*\)", string.Empty);
                if (result == input)
                    break;
                input = result;
            }

            return input;
        }

        public static int QuickDeriveTokenCount(string input)
        {
            return (int)(CountWords(input) * 0.75f);
        }

        private static readonly Random _random = new Random(Environment.TickCount);

        public static List<string> GetRandom(List<string> theList, int elementCount)
        {
            if (theList == null || theList.Count < elementCount)
                throw new ArgumentException($"List must contain at least {elementCount} elements.");

            return theList
                .OrderBy(_ => _random.Next())
                .Take(elementCount)
                .ToList();
        }

        public static int CountWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            // Matches words separated by whitespace/punctuation
            var matches = Regex.Matches(input, @"\b[\w']+\b");

            return matches.Count;
        }
        public static string ReplaceLfWithCrlf(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // First normalize any existing CRLF down to LF, then convert all LF to CRLF.
            // This avoids accidentally turning existing \r\n into \r\r\n.
            string normalized = input.Replace("\r\n", "\n");
            return normalized.Replace("\n", "\r\n");
        }

        public static string SuperTrim(string input) => input?.Trim().Trim('"', '\'', '.', ';', ',').Trim();

        public static string SmartExtractJson(string text)
        {
            var jsonMarker = "```json";
            var jsonMarker2 = "```";
            var t = text.Trim();
            if (t.StartsWith(jsonMarker))
            {
                t = t.Substring(jsonMarker.Length);
                if (t.EndsWith(jsonMarker2))
                    t = t.Substring(0, t.Length - jsonMarker2.Length);
            }
            else if(t.Contains("{"))
            {
                var firstCurly = t.IndexOf("{");
                var lastCurly = t.LastIndexOf("}");
                if (firstCurly >= 0 && lastCurly > firstCurly)
                    t = t.Substring(firstCurly, lastCurly - firstCurly + 1);
            }
            else if (t.Contains("["))
            {
                var firstCurly = t.IndexOf("[");
                var lastCurly = t.LastIndexOf("]");
                if (firstCurly >= 0 && lastCurly > firstCurly)
                    t = t.Substring(firstCurly, lastCurly - firstCurly + 1);
            }
            return t.Trim();
        }

        public static string RemoveMultiLineComment(string line)
        {
            char replacementChar = (char)1;
            var replacementCharStr = replacementChar.ToString();
            var sb = new StringBuilder(1024);
            sb.Append(line);
            var x = 0;
            var eraseMode = false;
            while (x < sb.Length)
            {
                if ((sb[x] == '/') && (x < sb.Length - 1) && (sb[x + 1] == '*'))
                {
                    eraseMode = true;
                    sb[x] = sb[x + 1] = replacementChar;
                }

                if (eraseMode && (sb[x] == '*') && (x < sb.Length - 1) && (sb[x + 1] == '/'))
                {
                    eraseMode = false;
                    sb[x] = sb[x + 1] = replacementChar;
                }

                if (eraseMode && sb[x] != 13 && sb[x] != 10)
                {
                    sb[x] = replacementChar;
                }
                x += 1;
            }
            return sb.ToString().Replace(replacementCharStr, "");
        }
    }
}
