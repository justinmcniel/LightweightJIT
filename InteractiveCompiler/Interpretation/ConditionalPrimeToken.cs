using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ConditionalPrimeToken
    {
        private class ValueComparison
        {
            public ValueToken? Value1;
            public ComparisonOperatorToken? Comparator;
            public ValueToken? Value2;
        }

        private ValueComparison? valCompare;
        private BooleanImmediateToken? booleanImmediate;
        private ConditionalFunctionCallToken? condFuncCall;

        public static ConditionalPrimeToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ConditionalPrimeToken res = new();
            int internalIndex = index;

            res.valCompare = new()
            {
                Value1 = ValueToken.TryParse(text, ref internalIndex, compiler),
                Comparator = ComparisonOperatorToken.TryParse(text, ref internalIndex, compiler),
                Value2 = ValueToken.TryParse(text, ref internalIndex, compiler)
            };
            if (res.valCompare.Value1 != null && res.valCompare.Comparator != null && res.valCompare.Value2 != null)
            {
                index = internalIndex;
                return res;
            }
            res.valCompare = null;
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
        public string Decompile(string indentation = "")
        {
            if (valCompare != null)
            { 
                return $"{valCompare.Value1!.Decompile(indentation)} " +
                    $"{valCompare.Comparator!.Decompile(indentation)} " +
                    $"{valCompare.Value2!.Decompile(indentation)}"; 
            }
            if (booleanImmediate != null)
            { return booleanImmediate.Decompile(indentation); }
            if(condFuncCall != null)
            { return condFuncCall.Decompile(indentation); }
            return "";
        }
    }
}
