using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ConditionalPrimeToken
    {
        private struct ValueComparison
        {
            public ValueToken? Value1;
            public ComparisonOperatorToken? Comparator;
            public ValueToken? Value2;

            public readonly bool HasValue() => Value1 != null && Comparator != null && Value2 != null;
        }

        private ValueComparison valCompare = new();
        private BooleanImmediateToken? booleanImmediate;
        private ConditionalFunctionCallToken? condFuncCall;

        public static ConditionalPrimeToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ConditionalPrimeToken res = new();
            int internalIndex = index;

            res.valCompare.Value1 = ValueToken.TryParse(text, ref internalIndex, compiler);
            res.valCompare.Comparator = ComparisonOperatorToken.TryParse(text, ref internalIndex, compiler);
            res.valCompare.Value2 = ValueToken.TryParse(text, ref internalIndex, compiler);
            if (res.valCompare.HasValue())
            {
                index = internalIndex;
                return res;
            }
            res.valCompare = new();
            internalIndex = index;

            res.booleanImmediate = BooleanImmediateToken.TryParse(text, ref internalIndex, compiler);
            if (res.booleanImmediate != null)
            {
                index = internalIndex;
                return res;
            }

            res.condFuncCall = ConditionalFunctionCallToken.TryParse(text, ref internalIndex, compiler);
            if (res.condFuncCall != null)
            {
                index = internalIndex;
                return res;
            }

            return null;
        }
    }
}
