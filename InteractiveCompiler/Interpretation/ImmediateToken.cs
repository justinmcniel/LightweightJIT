using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ImmediateToken
    {
        public object? Value { get; private set; }
        public static ImmediateToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ImmediateToken res = new();
            int internalIndex = index;

            if(Utilities.NextTokenMatches(text, ref internalIndex, "null"))
            {
                res.Value = null;
                index = internalIndex;
                return res;
            }

            BooleanImmediateToken? b = BooleanImmediateToken.TryParse(text, ref internalIndex, compiler);
            if (b != null && b.Value != null)
            {
                res.Value = b.Value;
                index = internalIndex;
                return res;
            }

            StringToken? s = StringToken.TryParse(text, ref internalIndex, compiler);
            if (s != null && s.Value != null)
            {
                res.Value = s.Value;
                index = internalIndex;
                return res;
            }

            NumericToken? n = NumericToken.TryParse(text, ref internalIndex, compiler);
            if (n != null && n.Value != null)
            {
                res.Value = n.Value;
                index = internalIndex;
                return res;
            }

            return null;
        }
    }
}
