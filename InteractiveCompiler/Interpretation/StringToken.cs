using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InteractiveCompiler.Interpretation
{
    internal class StringToken
    {
        public string? Value { get; private set; }
        public static StringToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            StringToken res = new();
            int internalIndex = index;

            if (Utilities.NextTokenMatches(text, ref internalIndex, "\""))
            {
                res.Value = "";
                try
                {
                    while (text[internalIndex] != '"')
                    { res.Value += Utilities.GetEscapeCharacter(text, ref internalIndex) ?? text[internalIndex++]; }
                    internalIndex++; //move past the '"'
                }
                catch (InvalidDataException)
                {
                    internalIndex++;
                    compiler.LogError($"ERROR: {Utilities.GetPosition(text, internalIndex)} " +
                        $"Unknown escape character \"\\{text[internalIndex]}\"");
                    return null;
                }
                catch (IndexOutOfRangeException)
                { return null; }
            }

            if (res.Value != null)
            {
                index = internalIndex;
                return res;
            }

            return null;
        }
    }
}
