using System;
using System.Collections.Generic;
using System.Text;

namespace fAI.Util.Strings
{
    public static class StringUtil
    {
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
