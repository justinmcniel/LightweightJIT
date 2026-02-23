using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    public static class Utilities
    {
        public static void SkipWhitespace(string text, ref int index)
        {
            try
            {
                while (Char.IsWhiteSpace(text[index]))
                { index++; }
            }
            catch (IndexOutOfRangeException) 
            { return; }
        }

        public static bool NextTokenMatches(string text, ref int index, string token, bool caseSensitive = true)
        {
            int internalIndex = index;
            SkipWhitespace(text, ref internalIndex);
            char HandleCaps(char c) => caseSensitive ? c : Char.ToLower(c);

            try
            {
                foreach (char c in token)
                {
                    if (HandleCaps(text[internalIndex++]) != HandleCaps(c))
                    {
                        return false;
                    }
                }
            }
            catch (IndexOutOfRangeException)
            { return false; }

            index = internalIndex;
            return true;
        }

        public static readonly string[] SymbolsList = [";", ":", "(", ")", "=", ",", "{", "}", "==", "!=", "<", "<=", ">", ">="];

        public static string NextTextToken(string text, ref int index)
        {
            string res = "";
            SkipWhitespace(text, ref index);
            try
            {
                while (Char.IsLetterOrDigit(text[index]) | !SymbolsList.Contains(text[index].ToString()))
                {
                    res += text[index++];
                }
            }
            catch (IndexOutOfRangeException) { }
            return res;
        }

        public static int NextInstanceOf(string text, int index, string token, bool caseSensitive = true)
        {
            if(String.IsNullOrEmpty(token))
            { return text.Length; }

            try
            {
                while (true)
                {
                    while (text[index] != token[0]) 
                    { index++; }

                    var internalIndex = index;
                    if(NextTokenMatches(text, ref internalIndex, token, caseSensitive: caseSensitive))
                    {
                        return index;
                    }
                    index++;
                }
            }
            catch (IndexOutOfRangeException)
            { return text.Length; }
        }
    }
}
