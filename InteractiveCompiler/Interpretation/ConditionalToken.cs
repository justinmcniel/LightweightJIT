using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ConditionalToken
    {
        private struct ValueComparison
        {
            public ValueToken? Value1;
            public ComparisonOperatorToken? Comparator;
            public ValueToken? Value2;

            public readonly bool HasValue() => Value1 != null && Comparator != null && Value2 != null;
        }

        private struct BooleanComparison
        {
            public ConditionalToken? Condition1;
            public BooleanOperatorToken? Operator;
            public ConditionalToken? Condition2;
            public readonly bool HasValue() => Condition1 != null && Operator != null && Condition2 != null;
        }

        private ValueComparison valCompare = new();
        private BooleanImmediateToken? booleanImmediate;
        private ConditionalFunctionCallToken? condFuncCall;
        private BooleanComparison boolCompare = new();

        public static ConditionalToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ConditionalToken res = new();
            int internalIndex = index;

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

            res.valCompare.Value1 = ValueToken.TryParse(text, ref internalIndex, compiler);
            res.valCompare.Comparator = ComparisonOperatorToken.TryParse(text, ref internalIndex, compiler);
            res.valCompare.Value2 = ValueToken.TryParse(text, ref internalIndex, compiler);
            if(res.valCompare.HasValue())
            {
                index = internalIndex;
                return res;
            }
            res.valCompare = new();
            internalIndex = index;

            res.boolCompare.Condition1 = ConditionalToken.TryParse(text, ref internalIndex, compiler); //infinite recursion?
            res.boolCompare.Operator = BooleanOperatorToken.TryParse(text, ref internalIndex, compiler);
            res.boolCompare.Condition2 = ConditionalToken.TryParse(text, ref internalIndex, compiler);
            if (res.boolCompare.HasValue())
            {
                index = internalIndex;
                return res;
            }
            //res.boolCompare = new(); //irrelevant
            //internalIndex = index; //irrelevant

            return null;
        }
    }
}
