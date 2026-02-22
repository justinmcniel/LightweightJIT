using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class NumericToken
    {
        public object? Value { get; private set; }
        public static NumericToken? TryParse(string text, ref int index, IInterractiveCompiler compiler)
        {
            NumericToken res = new();
            int internalIndex = index;

            if (Utilities.NextTokenMatches(text, ref internalIndex, "0x", caseSensitive: false)) 
            {
                HexToken? hex = HexToken.TryParse(text, ref internalIndex, compiler);
                if (hex != null)
                {
                    res.Value = hex.Value;
                    index = internalIndex;
                    return res;
                }
            }
            internalIndex = index;


            if (Utilities.NextTokenMatches(text, ref internalIndex, "0b", caseSensitive: false))
            {
                BinToken? hex = BinToken.TryParse(text, ref internalIndex, compiler);
                if (hex != null)
                {
                    res.Value = hex.Value;
                    index = internalIndex;
                    return res;
                }
            }
            internalIndex = index;

            FloatToken? flo = FloatToken.TryParse(text, ref internalIndex, compiler);
            if (flo != null)
            {
                res.Value = flo.Value;
                index = internalIndex;
                return res;
            }

            DecimalToken? dec = DecimalToken.TryParse(text, ref internalIndex, compiler);
            if (dec != null)
            {
                res.Value = dec.Value;
                index = internalIndex;
                return res;
            }

            return null;
        }
    }
}
