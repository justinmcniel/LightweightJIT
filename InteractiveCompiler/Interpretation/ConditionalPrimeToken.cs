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

            public bool HasValue() => Value1 != null && Comparator != null && Value2 != null;
        }

        private ValueComparison? valCompare;
        private BooleanImmediateToken? booleanImmediate;
        private ConditionalFunctionCallToken? condFuncCall;
        private bool inverted = false;

        public static ConditionalPrimeToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ConditionalPrimeToken res = new();
            int startingIndex = index;

            res.inverted = Utilities.NextTokenMatches(text, ref startingIndex, "!");
            int internalIndex = startingIndex;

            res.valCompare = new()
            {
                Value1 = ValueToken.TryParse(text, ref internalIndex, compiler),
                Comparator = ComparisonOperatorToken.TryParse(text, ref internalIndex, compiler),
                Value2 = ValueToken.TryParse(text, ref internalIndex, compiler)
            };
            if (res.valCompare.HasValue())
            {
                index = internalIndex;
                return res;
            }
            res.valCompare = null;
            internalIndex = startingIndex;

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

            if (res.inverted && res.valCompare != null)
            { throw new CompilerException(); }

            return null;
        }
        public string Decompile(string indentation = "")
        {
            string inversion = inverted ? "!" : "";
            if (valCompare != null)
            { 
                return inversion + 
                    $"{valCompare.Value1!.Decompile(indentation)} " +
                    $"{valCompare.Comparator!.Decompile(indentation)} " +
                    $"{valCompare.Value2!.Decompile(indentation)}"; 
            }
            if (booleanImmediate != null)
            { return inversion + booleanImmediate.Decompile(indentation); }
            if(condFuncCall != null)
            { return inversion + condFuncCall.Decompile(indentation); }
            return inversion;
        }

        public Func<bool> Compile(IInteractiveCompiler compiler)
        {
            Func<bool>? res = null;
            if (valCompare != null && valCompare.HasValue())
            {
                var Getter1 = valCompare.Value1!.Compile(compiler);
                var Getter2 = valCompare.Value2!.Compile(compiler);
                res =  valCompare.Comparator!.Compile(compiler, Getter1, Getter2);
            }
            else if (booleanImmediate != null)
            {
                res =  booleanImmediate.Compile(compiler);
            }
            else if (condFuncCall != null)
            {
                res =  condFuncCall.Compile(compiler);
            }

            if (inverted && res != null)
            {
                return () => !res();
            }

            return res ?? throw new CompilerException();
        }
    }
}
