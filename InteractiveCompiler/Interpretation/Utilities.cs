using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        public static string NextTokenReadable(string text, int index)
        {
            foreach(var symbol in SymbolsList)
            {
                if (NextTokenMatches(text, ref index, symbol))
                {
                    return ReadableSymbol(symbol);
                }
            }

            var res = NextTextToken(text, ref index);
            if (String.IsNullOrEmpty(res))
            { res = "END OF FILE"; }
            return res;
        }

        public static string ReadableSymbol(string symbol)
        {
            return $"{SymbolNames[symbol]} ({symbol})";
        }

        public static string GetPosition(string text, int index)
        {
            int line = 1;
            int pos = 0;
            int counter = 0;

            SkipWhitespace(text, ref index);

            foreach (var c in text)
            {
                pos++;

                if(c == '\n')
                {
                    line++;
                    pos = 0;
                }

                counter++;
                if (counter >= index)
                { break; }
            }
            if (line == 11)
            { Console.WriteLine(); }
            return $"{line}:{pos}";
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

        public static readonly string[] SymbolsList = [";", ":", "(", ")", "=", ",", "{", "}", "==", "!=", "<", "<=", ">", ">=", "!"];
        public static readonly Dictionary<string, string> SymbolNames = new()
        {
            {";", "SEMICOLON"},
            {":", "COLON"},
            {"(", "OPEN_PARENTHASES"},
            {")", "CLOSE_PARENTHASES"},
            {"=", "EQUALS_SIGN"},
            {",", "COMMA"},
            {"{", "OPEN_CURLY_BRACES"},
            {"}", "CLOSE_CURLY_BRACES"},
            {"==", "EQUALS_COMPARATOR"},
            {"!=", "NOT_EQUALS_COMPARATOR"},
            {"<", "LEFT_CARRET"},
            {"<=", "LESS_THAN_OR_EQUAL_TO_COMPARATOR"},
            {">", "RIGHT_CARRET"},
            {">=", "GREATER_THAN_OR_EQUAL_TO_COMPARATOR"},
            {"!", "EXCLAMATION_POINT"},
        };

        public static string NextTextToken(string text, ref int index)
        {
            string res = "";
            SkipWhitespace(text, ref index);
            try
            {
                while ((Char.IsLetterOrDigit(text[index]) || !SymbolsList.Contains(text[index].ToString())) && !Char.IsWhiteSpace(text[index]))
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

        public static char? GetEscapeCharacter(string text, ref int index)
        {
            if (text[index] != '\\')
            { return null; }

            char? ErrorHandler(ref int i)
            { i -= 2; throw new InvalidDataException(); }

            char? HexHandler(ref int i)
            {
                if (!Char.IsAsciiHexDigit(text[i]))
                { return null; }

                if (Char.IsAsciiHexDigit(text[i + 1]))
                {
                    i += 2;
                    return (char?)Convert.ToInt16(text[(i - 2)..i], 16);
                }
                else
                {
                    i++;
                    return (char?)Convert.ToInt16($"{text[i - 1]}", 16);
                }
            }

            index++;
            return text[index++] switch
            {
                '\\' => '\\',
                '\'' => '\'',
                '\"' => '\"',
                'r' => '\r',
                'n' => '\n',
                't' => '\t',
                '0' => '\0',
                'a' => '\a',
                'b' => '\b',
                'f' => '\f',
                'v' => '\v',
                'x' => HexHandler(ref index),
                _ => ErrorHandler(ref index)
            };
        }
    }
}
