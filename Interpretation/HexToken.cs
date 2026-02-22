using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class HexToken
    {
        public int? Value { get; private set; }
        public static HexToken? TryParse(string text, ref int index, IInterractiveCompiler compiler)
        {
            HexToken res = new();
            int internalIndex = index;

            string val = "";
            try
            {
                while (Char.IsAsciiHexDigit(text[internalIndex]))
                { val += text[internalIndex++]; }
            }
            catch (IndexOutOfRangeException)
            { return null; }

            if (!string.IsNullOrEmpty(val))
            {
                try
                { res.Value = Convert.ToInt32(val, 16); }
                catch { return null; }

                index = internalIndex;
                return res;
            }

            return null;
        }
    }
}
