using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class FloatToken
    {
        public float? Value { get; private set; }
        public static FloatToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            FloatToken res = new();
            int internalIndex = index;

            DecimalToken? d1 = DecimalToken.TryParse(text, ref internalIndex, compiler);
            if (d1 != null)
            {
                if (!Utilities.NextTokenMatches(text, ref internalIndex, "."))
                {
                    DecimalToken? d2 = DecimalToken.TryParse(text, ref internalIndex, compiler);
                    if (d2 != null)
                    {
                        res.Value = float.Parse($"{d1.Value}.{d2.Value}");
                    }
                }
            }

            if (res.Value == null)
            { return null; }
            return res;
        }
    }
}
